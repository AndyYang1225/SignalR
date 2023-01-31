using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace SignalR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private JWTOption _jwtOpt;
        // 等待服務容器注入 IOptions<JWTOption>
        public TokenController(IOptions<JWTOption> jwtOpt)
        {
            this._jwtOpt = jwtOpt.Value;
        }

        [AllowAnonymous]
        [HttpPost("signin")]
        public IActionResult SignIn(LoginModel loginModel)
        {
            // 模擬驗證使用者帳號密碼
            var canLogin = loginModel.userId == "ktgh" && loginModel.password == "pass";
            if (canLogin)
            {
                string issuer = _jwtOpt.Issuer;
                string signKey = _jwtOpt.SignKey;

                // 設定要加入到 JWT Token 中的聲明資訊(Claims)
                List<Claim> claims = new List<Claim>();

                // 加入Sub(用戶)
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, loginModel.userId));
                // 加入jti(JWT ID) 用於一次性 token
                //claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

                // 聲明使用者 userId、roles 的 claim，這裡會用來判斷使用者識別碼、使用者群組
                claims.Add(new Claim("userId", loginModel.userId));
                claims.Add(new Claim("roles", "Admin"));
                claims.Add(new Claim("roles", "Users"));

                // 建立一組對稱式加密金鑰，主要用於 JWT 簽章之用
                SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signKey));
                // 定義加密金鑰、演算法、數位簽章摘要
                // HmacSha256 必須大於 128 bits，亦即 key 長度至少要 16 字元
                SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

                // 產生一個 JwtSecurityToken
                var token = new JwtSecurityToken(
                        issuer,  // Issuer    
                        issuer,  // Audience    
                        claims,  // claims
                        expires: DateTime.Now.AddMinutes(_jwtOpt.Expires), // token 生效至過期的分鐘數
                        signingCredentials: signingCredentials
                        );
                // 序列化 JwtSecurityToken
                var jwt_token = new JwtSecurityTokenHandler().WriteToken(token);

                // 回傳 token
                return Ok(new
                {
                    token = jwt_token,
                });
            }
            // 驗證失敗
            return BadRequest(new
            {
                err = "登入失敗",
            });
        }
    }
    public class LoginModel
    {
        public string userId { get; set; }
        public string password { get; set; }
    }
}