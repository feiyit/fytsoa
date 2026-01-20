using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FytSoa.Common.Utils;
using FytSoa.Common.Jwt.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FytSoa.Common.Jwt;

public class JwtAuthService
{
    public static string IssueJwt (JwtToken token) {
        var jwtModel = AppUtils.GetConfig(JwtModel.Name).Get<JwtModel> ();
        var claims = new List<Claim> ();
        //每次登陆动态刷新
        //JwtConst.ValidAudience = token.Id + DateTime.Now.ToString(CultureInfo.InvariantCulture);
        claims.AddRange (new [] {
            new Claim (nameof (JwtToken.Id), token.Id.ToString()),
            new Claim (nameof (JwtToken.EmployeeId), token.EmployeeId.ToString()),
            new Claim (nameof (JwtToken.TenantId), token.TenantId.ToString()),
            new Claim (nameof (JwtToken.FullName), token.FullName),
            new Claim (nameof (JwtToken.RoleArray), token.RoleArray),
            new Claim (nameof (JwtToken.Time), token.Time.ToString (CultureInfo.InvariantCulture)),
            new Claim (ClaimTypes.Role, token.Role)
        });
        var key = new SymmetricSecurityKey (Encoding.UTF8.GetBytes (jwtModel.Security));
        var cred = new SigningCredentials (key, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken (
            issuer: jwtModel.Issuer,
            //audience: JwtConst.ValidAudience,
            audience : jwtModel.Audience,
            claims : claims,
            expires : DateTime.Now.AddMinutes (jwtModel.WebExp),
            signingCredentials : cred);
        return new JwtSecurityTokenHandler ().WriteToken (jwt);
    }

    /// <summary>
    /// 解析
    /// </summary>
    /// <param name="jwtStr"></param>
    /// <returns></returns>
    public static JwtToken SerializeJwt (string jwtStr) {
        var jwtHandler = new JwtSecurityTokenHandler ();
        var jwtToken = jwtHandler.ReadJwtToken (jwtStr);
        object? userName;
        object? roleArray;
        object? time;
        object? id;
        object? employeeId;
        object? tenantId;
        object? role;
        try {
            jwtToken.Payload.TryGetValue ("FullName", out userName);
            jwtToken.Payload.TryGetValue ("RoleArray", out roleArray);
            jwtToken.Payload.TryGetValue ("Time", out time);
            jwtToken.Payload.TryGetValue ("Id", out id);
            jwtToken.Payload.TryGetValue ("EmployeeId", out employeeId);
            jwtToken.Payload.TryGetValue ("TenantId", out tenantId);
            jwtToken.Payload.TryGetValue (ClaimTypes.Role, out role);
        } catch (Exception e) {
            Console.WriteLine (e);
            throw;
        }
        return new JwtToken()
        {
            Id = Convert.ToInt64(id),
            TenantId = Convert.ToInt64(tenantId),
            EmployeeId = Convert.ToInt64(employeeId),
            FullName = userName?.ToString (),
            RoleArray = roleArray?.ToString(),
            Role = role?.ToString(),
            Time = Convert.ToDateTime (time)
        };
    }
}