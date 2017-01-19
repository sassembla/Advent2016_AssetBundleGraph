using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

/**
Example code for asset bundle build postprocess.
*/
public class MyPostprocess0 : AssetBundleGraph.IPostprocess {
	/* 
	 * DoPostprocess() is called when build performed.
	 * @param [in] reports	collection of AssetBundleBuildReport from each BundleBuilders.
	 */
	public void DoPostprocess (IEnumerable<AssetBundleGraph.AssetBundleBuildReport> buildReports, IEnumerable<AssetBundleGraph.ExportReport> exportReports) {
		
		StringBuilder sb = new StringBuilder();
		
		foreach (var report in buildReports) {

			sb.AppendFormat("BUILD REPORT({0}):\n-------\n", report.Node.Name);

			foreach(var v in report.BuiltBundleFiles) {
				sb.AppendFormat("{0}\n", v.fileNameAndExtension);
			}

			sb.Append("-------\n");
			Debug.Log(sb.ToString());
		}

		foreach (var export in exportReports) {

			sb.AppendFormat("EXPORT REPORT({0}):\n\n", export.Node.Name);

			foreach(var v in export.ExportedItems) {
				sb.AppendFormat("{0} => {1}\n", v.source, v.destination);
			}

			if(export.Errors.Count > 0) {
				sb.Append("\n-- ERRORS -----\n\n");

				foreach(var v in export.Errors) {
					sb.AppendFormat("{0} => {1} : {2}\n", v.source, v.destination, v.reason);
				}
			}

			sb.Append("-------\n");
			Debug.Log(sb.ToString());
		}

		/*
			このあたりに自前でコードを書くことで、
			
			ビルド -> 完了した結果を外部のサービスへと通知
			というようなことができる。

			で、ここでは架空のメールアドレスにBuild結果を投稿してみる。
			ちなみに下記のコードでgmailとかに投げるとすごく怒られるので各位工夫してね。参考までに、hotmailとか意識がダメそうなやつは通った。
		*/
		Debug.Log("postprocess done.");
		// var smtpClientHost = "VVV.WWW.com";//　smtp.live.comとか
		// var mailAddress = "XXXXXXX@YYYYYYY.com";
		// var password = "ZZZZZZ";
		// var body = sb.ToString();

		// MailMessage mail = new MailMessage();

		// mail.From = new MailAddress(mailAddress);
		// mail.To.Add(mailAddress);
		// mail.Subject = "AssetBundleGraph build is over.";
		// mail.Body = body;

		// SmtpClient smtpServer = new SmtpClient(smtpClientHost);
		// smtpServer.Port = 587;
		// smtpServer.Credentials = new System.Net.NetworkCredential(mailAddress, password) as ICredentialsByHost;
		// smtpServer.EnableSsl = true;
		// ServicePointManager.ServerCertificateValidationCallback = 
		// delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
		// 	return true;
		// };
		// smtpServer.Send(mail);

		/*
			今回は適当なメールを送るのみにしたけれど、例えば全てのAssetに対してimporterFromをキーに整理すると、
			とあるpathにあるAssetがどんなNodeを通過したのか、などが把握できる。
		*/
	}
}
