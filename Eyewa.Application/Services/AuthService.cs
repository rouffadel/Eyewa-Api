using static Eyewa.Application.DTOs.Common;
using Eyewa.Domain.Entities;
using Eyewa.Application.Interfaces;
using Eyewa.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;


namespace Eyewa.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IDbLoggerService _dbLogger;
        private readonly IDbExecutorService _dbExecutor;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IApplicationDbContext context,
            IConfiguration configuration,
            IDbLoggerService dbLogger,
            IDbExecutorService dbExecutor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _configuration = configuration;
            _dbLogger = dbLogger;
            _dbExecutor = dbExecutor;
        }

        private string GenerateJwtToken(string username, string loginId, string roleId, string storeId, string tenantId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "SuperSecretKeyForEyewaSalesApi2026!");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim("loginId", loginId),
                    new Claim("roleId", roleId),
                    new Claim("storeId", storeId),
                    new Claim("tenantId", tenantId)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _configuration["Jwt:Issuer"] ?? "EyewaSalesApi",
                Audience = _configuration["Jwt:Audience"] ?? "EyewaSalesClient",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task<string> GenerateRefreshToken(string userId, string ipAddress)
        {
            var tokenBytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBytes);
            }
            var tokenString = Convert.ToBase64String(tokenBytes);

            var refreshToken = new UserRefreshToken
            {
                UserId = userId,
                Token = tokenString,
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                ReplacedByToken = "",
                RevokedByIp = ""
            };

            _context.UserRefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return tokenString;
        }

        public async Task<List<Dictionary<string, object>>> Echeckpermissions(int roleId)
        {
            string screenUrl = "Sales.aspx";
            var whereCondition = " and  R.RoleId='" + roleId + "' and SM.ScreenUrl like '../Screens/" + screenUrl + "%'";
            var parameters = new Dictionary<string, object?>
            {
                { "@wherecondition", whereCondition },
                { "@Transaction", "Checkpermissions" }
            };
            return await _dbExecutor.ExecuteStoredProcedureAsync("SP_GetDataRoleScreenMapping", parameters);
        }

        public async Task<TransactResult> Register(string username, string password, int roleId)
        {
            try
            {
                var user = new ApplicationUser
                {
                    UserName = username,
                    RoleId = roleId
                };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    return new TransactResult { Status = "200", Message = "User registered successfully", objresult = user };
                }

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new TransactResult { Status = "-100", Message = errors, objresult = null };
            }
            catch (Exception ex)
            {
                return new TransactResult { Status = "-100", Message = ex.Message };
            }
        }

        public async Task<TransactResult> LoginWithIdentity(string username, string password, string ipAddress)
        {
            try
            {
                _dbLogger.LogInfo("LoginWithIdentity started for user: " + username);
                TransactResult tres = new TransactResult();

                var user = await _userManager.FindByNameAsync(username);
                if (user == null)
                {
                    tres.Status = "-100";
                    tres.Message = "Invalid credentials";
                    return tres;
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
                if (!result.Succeeded)
                {
                    tres.Status = "-100";
                    tres.Message = "Invalid credentials";
                    return tres;
                }

                var loginObj = new LoginCls();
                var permissionList = await Echeckpermissions(user.RoleId);
                if (permissionList != null && permissionList.Count > 0)
                {
                    var firstRow = permissionList[0];
                    loginObj.Add = Convert.ToBoolean(firstRow["ADD"]?.ToString());
                    loginObj.View = Convert.ToBoolean(firstRow["VIEW"]?.ToString());
                    loginObj.Delete = Convert.ToBoolean(firstRow["DELETE"]?.ToString());
                    loginObj.Edit = Convert.ToBoolean(firstRow["EDIT"]?.ToString());
                }
                else
                {
                    loginObj.View = true;
                    loginObj.Add = true;
                    loginObj.Delete = true;
                    loginObj.Edit = true;
                }

                var userDetail = new LoginUserDetailDto
                {
                    LoginID = 2,
                    RoleID = user.RoleId,
                    UserName = user.UserName,
                    StoreID = 0,
                    TenantId = user.TenantId
                };

                string accessToken = GenerateJwtToken(user.UserName, "2", user.RoleId.ToString(), "0", user.TenantId);
                string refreshToken = await GenerateRefreshToken(user.Id, ipAddress);

                loginObj.Token = accessToken;
                loginObj.RefreshToken = refreshToken;
                loginObj.result = userDetail;

                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = loginObj;

                return tres;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("LoginWithIdentity error: " + ex.Message, ex.StackTrace);
                return new TransactResult { Status = "-100", Message = ex.Message };
            }
        }

        public async Task<TransactResult> RefreshToken(string token, string ipAddress)
        {
            try
            {
                var refreshToken = _context.UserRefreshTokens.FirstOrDefault(t => t.Token == token);
                if (refreshToken == null || !refreshToken.IsActive)
                {
                    return new TransactResult { Status = "-100", Message = "Invalid or expired refresh token" };
                }

                ApplicationUser user = null;
                if (int.TryParse(refreshToken.UserId, out int parsedLegacyId))
                {
                    user = await _userManager.Users.FirstOrDefaultAsync(u => u.LegacyLoginId == parsedLegacyId);
                }
                if (user == null)
                {
                    user = await _userManager.FindByIdAsync(refreshToken.UserId);
                }

                string username, loginId, roleId, storeId, tenantId;

                if (user != null)
                {
                    username = user.UserName;
                    loginId = user.LegacyLoginId.ToString();
                    roleId = user.RoleId.ToString();
                    storeId = "0";
                    tenantId = user.TenantId;
                }
                else
                {
                    var parameters = new Dictionary<string, object?>
                    {
                        { "@WhereCondition", " and L.LoginID=" + refreshToken.UserId },
                        { "@Transaction", "VerifyRoleswiseUserlogin" }
                    };
                    var legacyList = await _dbExecutor.ExecuteStoredProcedureAsync("SP_GetDataLogin", parameters);

                    if (legacyList != null && legacyList.Count > 0)
                    {
                        var row = legacyList[0];
                        username = row["LoginName"]?.ToString() ?? "";
                        loginId = refreshToken.UserId;
                        roleId = row["RoleID"]?.ToString() ?? "";
                        storeId = row.ContainsKey("StoreID") ? row["StoreID"]?.ToString() ?? "" : "";
                        tenantId = row.ContainsKey("TenantId") ? row["TenantId"]?.ToString() ?? "" : "";
                    }
                    else
                    {
                        return new TransactResult { Status = "-100", Message = "User not found" };
                    }
                }

                string newAccessToken = GenerateJwtToken(username, loginId, roleId, storeId, tenantId);
                string newRefreshTokenString = await GenerateRefreshToken(loginId, ipAddress);

                refreshToken.Revoked = DateTime.UtcNow;
                refreshToken.RevokedByIp = ipAddress;
                refreshToken.ReplacedByToken = newRefreshTokenString;

                _context.UserRefreshTokens.Update(refreshToken);
                await _context.SaveChangesAsync();

                var loginObj = new LoginCls
                {
                    Token = newAccessToken,
                    RefreshToken = newRefreshTokenString,
                    View = true,
                    Add = true,
                    Edit = true,
                    Delete = true
                };

                return new TransactResult { Status = "200", Message = "Success", objresult = loginObj };
            }
            catch (Exception ex)
            {
                return new TransactResult { Status = "-100", Message = ex.Message };
            }
        }

        public async Task<TransactResult> RevokeToken(string token, string ipAddress)
        {
            try
            {
                var refreshToken = _context.UserRefreshTokens.FirstOrDefault(t => t.Token == token);
                if (refreshToken == null || !refreshToken.IsActive)
                {
                    return new TransactResult { Status = "-100", Message = "Token is not active" };
                }

                refreshToken.Revoked = DateTime.UtcNow;
                refreshToken.RevokedByIp = ipAddress;

                _context.UserRefreshTokens.Update(refreshToken);
                await _context.SaveChangesAsync();

                return new TransactResult { Status = "200", Message = "Token revoked successfully" };
            }
            catch (Exception ex)
            {
                return new TransactResult { Status = "-100", Message = ex.Message };
            }
        }

        public async Task<TransactResult> VerifyUserLogin(string loginName, string password, string ipAddress)
        {
            try
            {
                _dbLogger.LogInfo("VerifyUserLogin Method was started, LoginName=" + loginName);
                TransactResult tres = new TransactResult();
                
                if (string.IsNullOrEmpty(loginName))
                {
                    return new TransactResult { Status = "-100", Message = "Login Name is mandatory" };
                }
                if (string.IsNullOrEmpty(password))
                {
                    return new TransactResult { Status = "-100", Message = "Password is mandatory" };
                }

                var whereCondition = " and L.LoginName='" + loginName + "' and L.password='" + password + "' ";
                var parameters = new Dictionary<string, object?>
                {
                    { "@WhereCondition", whereCondition },
                    { "@Transaction", "VerifyRoleswiseUserlogin" }
                };
                
                var resultList = await _dbExecutor.ExecuteStoredProcedureAsync("SP_GetDataLogin", parameters);
                
                if (resultList != null && resultList.Count > 0)
                {
                    var row = resultList[0];
                    int roleId = Convert.ToInt32(row["RoleId"]);
                    int loginId = Convert.ToInt32(row["LoginID"]);

                    var loginObj = new LoginCls();
                    if (loginId != 1)
                    {
                        var permissionList = await Echeckpermissions(roleId);
                        if (permissionList != null && permissionList.Count > 0)
                        {
                            loginObj.Add = Convert.ToBoolean(permissionList[0]["ADD"]?.ToString());
                            loginObj.View = Convert.ToBoolean(permissionList[0]["VIEW"]?.ToString());
                            loginObj.Delete = Convert.ToBoolean(permissionList[0]["DELETE"]?.ToString());
                            loginObj.Edit = Convert.ToBoolean(permissionList[0]["EDIT"]?.ToString());
                        }
                    }
                    else
                    {
                        loginObj.View = true;
                        loginObj.Add = true;
                        loginObj.Delete = true;
                        loginObj.Edit = true;
                    }

                    string tenantId = row.ContainsKey("TenantId") ? row["TenantId"]?.ToString() ?? "" : "";
                    if (string.IsNullOrEmpty(tenantId))
                    {
                        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.LegacyLoginId == loginId);
                        tenantId = user?.TenantId ?? Guid.NewGuid().ToString();
                    }

                    var userDetail = new LoginUserDetailDto
                    {
                        LoginID = loginId,
                        RoleID = roleId,
                        UserName = row["LoginName"]?.ToString() ?? loginName,
                        StoreID = row.ContainsKey("StoreID") && row["StoreID"] != null ? Convert.ToInt32(row["StoreID"]) : 0,
                        TenantId = tenantId
                    };

                    loginObj.Token = GenerateJwtToken(userDetail.UserName, userDetail.LoginID.ToString(), userDetail.RoleID.ToString(), userDetail.StoreID.ToString(), userDetail.TenantId);
                    loginObj.RefreshToken = await GenerateRefreshToken(userDetail.LoginID.ToString(), ipAddress);
                    loginObj.result = userDetail;

                    tres.Status = "200";
                    tres.Message = "Success";
                    tres.objresult = loginObj;
                }
                else
                {
                    tres.Status = "-100";
                    tres.Message = "Invalid credentials";
                }
                
                return tres;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("VerifyUserLogin Method error: " + ex.Message, ex.StackTrace);
                return new TransactResult { Status = "-100", Message = ex.Message };
            }
        }

        public async Task<TransactResult> GetUsers(int loginId, int storeId)
        {
            TransactResult tres = new TransactResult();
            try
            {
                _dbLogger.LogInfo("GetUsers Method was started");

                var parameters = new Dictionary<string, object?>
                {
                    { "@WhereCondition", "" },
                    { "@Transaction", "ddlUser" }
                };
                
                var result = await _dbExecutor.ExecuteStoredProcedureAsync("SP_GetDataSales", parameters);
                
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = result;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("GetUsers Method error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        public async Task<TransactResult> GetSalesMan(int loginId, int storeId)
        {
            TransactResult tres = new TransactResult();
            try
            {
                _dbLogger.LogInfo("GetSalesMan Method was started");
                var whereCondition = string.Empty;
                var transaction = string.Empty;

                if (loginId == 1)
                {
                    transaction = "ddlSalesMan";
                    if (storeId != 0)
                        whereCondition = " and StoreID =" + storeId;
                }
                else
                {
                    if (storeId != 0)
                        whereCondition = " and L.StoreID =" + storeId;
                    transaction = "ddlSalesManForUser";
                    whereCondition += " and L.LoginID=" + loginId;
                }

                var parameters = new Dictionary<string, object?>
                {
                    { "@WhereCondition", whereCondition },
                    { "@Transaction", transaction }
                };
                
                var result = await _dbExecutor.ExecuteStoredProcedureAsync("SP_GetDataSales", parameters);
                
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = result;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("GetSalesMan Method error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        public async Task SyncLegacyUsersAsync()
        {
            try
            {
                _dbLogger.LogInfo("SyncLegacyUsersAsync: Starting database user synchronization...");
                
                // Get all active, non-deleted users from Logins table.
                var legacyUsers = await _dbExecutor.ExecuteQueryAsync(
                    "SELECT LoginID, UserName, LoginName, Password, RoleID, Email, Active, Deleted, TenantId FROM Logins"
                );
                
                if (legacyUsers == null || legacyUsers.Count == 0)
                {
                    _dbLogger.LogInfo("SyncLegacyUsersAsync: No users found in the legacy Logins table.");
                    return;
                }
                
                _dbLogger.LogInfo($"SyncLegacyUsersAsync: Found {legacyUsers.Count} legacy users to check.");
                
                int createdCount = 0;
                int skippedCount = 0;
                int errorCount = 0;

                foreach (var legacyUser in legacyUsers)
                {
                    var loginName = legacyUser["LoginName"]?.ToString()?.Trim();
                    var password = legacyUser["Password"]?.ToString()?.Trim();
                    var email = legacyUser["Email"]?.ToString()?.Trim();
                    var roleIdVal = legacyUser["RoleID"];
                    int roleId = 2;
                    if (roleIdVal != null && int.TryParse(roleIdVal.ToString(), out int parsedRole))
                    {
                        roleId = parsedRole;
                    }

                    if (string.IsNullOrEmpty(loginName))
                    {
                        loginName = legacyUser["UserName"]?.ToString()?.Trim();
                    }

                    if (string.IsNullOrEmpty(loginName))
                    {
                        skippedCount++;
                        continue;
                    }

                    int loginId = Convert.ToInt32(legacyUser["LoginID"]);

                    // Check if this user already exists in AspNetUsers by LegacyLoginId
                    var existingUser = await _userManager.Users.FirstOrDefaultAsync(u => u.LegacyLoginId == loginId);
                    if (existingUser != null)
                    {
                        skippedCount++;
                        continue;
                    }

                    // Validate password
                    if (string.IsNullOrEmpty(password))
                    {
                        password = "Password123"; // fallback password if empty
                    }

                    string tenantId = legacyUser.ContainsKey("TenantId") && legacyUser["TenantId"] != null && !string.IsNullOrEmpty(legacyUser["TenantId"].ToString())
                        ? legacyUser["TenantId"].ToString()
                        : Guid.NewGuid().ToString();

                    // Generate a unique login name if duplicate
                    var userByName = await _userManager.FindByNameAsync(loginName);
                    string uniqueLoginName = loginName;
                    if (userByName != null)
                    {
                        uniqueLoginName = $"{loginName}_{loginId}";
                    }

                    var newUser = new ApplicationUser
                    {
                        UserName = uniqueLoginName,
                        Email = string.IsNullOrEmpty(email) ? $"{loginName}@eyewa.com" : email,
                        RoleId = roleId,
                        EmailConfirmed = true,
                        TenantId = tenantId,
                        LegacyLoginId = loginId
                    };

                    var result = await _userManager.CreateAsync(newUser, password);
                    if (result.Succeeded)
                    {
                        createdCount++;
                        // If we generated a new TenantId, make sure to update it in the Logins table
                        if (!legacyUser.ContainsKey("TenantId") || legacyUser["TenantId"] == null || string.IsNullOrEmpty(legacyUser["TenantId"].ToString()))
                        {
                            await _dbExecutor.ExecuteQueryAsync(
                                "UPDATE Logins SET TenantId = @TenantId WHERE LoginID = @LoginID",
                                new Dictionary<string, object?> { { "@TenantId", tenantId }, { "@LoginID", loginId } }
                            );
                        }
                    }
                    else
                    {
                        errorCount++;
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        _dbLogger.LogError($"SyncLegacyUsersAsync: Failed to create user '{uniqueLoginName}': {errors}", null);
                    }
                }
                
                _dbLogger.LogInfo($"SyncLegacyUsersAsync Completed: Created={createdCount}, Skipped={skippedCount}, Errors={errorCount}");
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("SyncLegacyUsersAsync error: " + ex.Message, ex.StackTrace);
            }
        }
    }
}





