package com.aliyun.logservice;

import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.JSONObject;
import com.aliyuncs.DefaultAcsClient;
import com.aliyuncs.exceptions.ClientException;
import com.aliyuncs.http.MethodType;
import com.aliyuncs.profile.DefaultProfile;
import com.aliyuncs.profile.IClientProfile;
import com.aliyuncs.sts.model.v20150401.AssumeRoleRequest;
import com.aliyuncs.sts.model.v20150401.AssumeRoleResponse;
import org.apache.http.HttpResponse;
import org.apache.http.client.ClientProtocolException;
import org.apache.http.client.HttpClient;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.impl.client.CloseableHttpClient;
import org.apache.http.impl.client.HttpClients;
import org.apache.http.util.EntityUtils;

import javax.swing.plaf.synth.SynthTextAreaUI;
import java.io.IOException;
import java.io.UnsupportedEncodingException;
import java.net.URLEncoder;

import static java.lang.System.exit;

/**
 * Hello world!
 *
 */
public class slsconsole
{
    public static void main(String[] args)
    {
        String akId = "";
        String ak = "";
        // 如何使用sts访问日志服务请参考：https://www.atatech.org/articles/101998
        String roleArn = "<roleArn名称>";
        String roleSession = "console-role-session";
        String stsHost = "sts.aliyuncs.com";

        String signInHost = "http://signin.aliyun.com";

        try
        {
            DefaultProfile.addEndpoint("", "", "Sts", stsHost);
            IClientProfile profile = DefaultProfile.getProfile("", akId, ak);
            DefaultAcsClient client = new DefaultAcsClient(profile);

            AssumeRoleRequest assumeRoleReq = new AssumeRoleRequest();
            assumeRoleReq.setRoleArn(roleArn);
            assumeRoleReq.setRoleSessionName(roleSession);
            assumeRoleReq.setMethod(MethodType.POST);
            assumeRoleReq.setDurationSeconds(3600L);
            // 默认可以不需要setPolicy，即申请获得角色的所有权限
            assumeRoleReq.setPolicy(本次生成token实际需要的权限字符串，申请权限必须是角色对应权限的子集); // 权限示例参考链接：https://help.aliyun.com/document_detail/89676.html

            AssumeRoleResponse assumeRoleRes = client.getAcsResponse(assumeRoleReq);
            System.out.println(assumeRoleRes.getCredentials().getAccessKeyId());
            System.out.println(assumeRoleRes.getCredentials().getAccessKeySecret());
            System.out.println(assumeRoleRes.getCredentials().getExpiration());
            System.out.println(assumeRoleRes.getCredentials().getSecurityToken());

            // construct singin url
            String signInTokenUrl = signInHost + String.format(
                    "/federation?Action=GetSigninToken"
                    + "&AccessKeyId=%s"
                    + "&AccessKeySecret=%s"
                    + "&SecurityToken=%s&TicketType=mini",
                    URLEncoder.encode(assumeRoleRes.getCredentials().getAccessKeyId(), "utf-8"),
                    URLEncoder.encode(assumeRoleRes.getCredentials().getAccessKeySecret(), "utf-8"),
                    URLEncoder.encode(assumeRoleRes.getCredentials().getSecurityToken(), "utf-8")
                    );

            System.out.println(signInTokenUrl);
            HttpGet signInGet = new HttpGet(signInTokenUrl);
            CloseableHttpClient httpClient = HttpClients.createDefault();
            HttpResponse httpResponse = httpClient.execute(signInGet);
            String signInToken = "";
            if (httpResponse.getStatusLine().getStatusCode() == 200)
            {
                String signInRes = EntityUtils.toString(httpResponse.getEntity());
                System.out.println(signInRes);
                signInToken = JSON.parseObject(signInRes).getString("SigninToken");

                if (signInToken == null)
                {
                    System.out.println("Invalid response message, contains no SigninToken: " + signInRes);
                    exit(-1);
                }
            }
            else
            {
                System.out.println("Failed to retrieve signInToken");
                exit(-1);
            }

            // construct final url
            String signInUrl = signInHost + String.format(
                    "/federation?Action=Login"
                    + "&LoginUrl=%s"
                    + "&Destination=%s"
                    + "&SigninToken=%s",
                    URLEncoder.encode("https://www.aliyun.com", "utf-8"),
                    URLEncoder.encode("https://sls4service.console.aliyun.com/next/project/ali-cn-hangzhou-sls-admin/logsearch/sls_operation_log?isShare=true&hideTopbar=true&hideSidebar=true", "utf-8"),
                    URLEncoder.encode(signInToken, "utf-8"));
            System.out.println(signInUrl);
        }
        catch (ClientException e)
        {
            e.printStackTrace();
        }
        catch (UnsupportedEncodingException e)
        {
            e.printStackTrace();
        }
        catch (ClientProtocolException e)
        {
            e.printStackTrace();
        }
        catch (IOException e)
        {
            e.printStackTrace();
        }
    }
}