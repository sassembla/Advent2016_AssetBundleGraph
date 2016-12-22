using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

/**
Example code for asset bundle build postprocess.
*/
public class MyPostprocess : AssetBundleGraph.IPostprocess {
	/* 
	 * Run() is called when build or dry-run is performed. 
	 * 
	 * @param [in] assetGroups	Dictionary of asset groups per Node 
	 * @param [in] isRun		True if this is actual build. 
	 */
	public void Run (Dictionary<AssetBundleGraph.NodeData, Dictionary<string, List<AssetBundleGraph.Asset>>> assetGroups, bool isRun) {

		// すべてのAssetBundleGraphのノードの処理が終わった後に、このメソッドが着火される。
		// bool isRun がtrueの時は実行時、falseの時は非実行時を表している。

		if (!isRun) {
			return;
		}
		
		// Buildボタンを押したあと = 実行時のみ、ここから先が実行される。
		Debug.Log("BUILD REPORT:");

		/*
			このメソッドで取得できる情報 assetGroups は、かなり巨大なツリー構造になっている。

			assetGroups(nodes)
				/ node	実行された順に、AssetBundleGraphのnodeのインスタンスが入っている

					/ groupKeys 実行時に入力されたGroup keyが、入力順に入っている

						/ groupKey 各Group key
						
							/ assets ノードへと、このGroup Keyで入力されたAssetが入っている

								/ asset Assetのインスタンス。次のようなパラメータを持っている。 readonly。

									string assetDatabaseId 		AssetのAssetDatabase内でのId
									string absoluteAssetPath 	Assetの絶対パス
									string importFrom 			AssetのAssets以下のパス
									string exportTo 			Assetがexportされたパス(exportされていれば記録される)
									Type assetType 				Assetのtype
									bool isNew 					このAssetが更新されたかどうかを判断するbool trueであれば前回の実行から変更がある
									bool isBundled 				AssetBundleにされたかどうか(AssetBundleの参照関係に関連)
									string variantName 			このAssetに対して使用されたvariant名
		*/
		
		var sb = new StringBuilder();

		// 特定のノードでどんな素材がInputされたか、というのを判断することができる。
		foreach (var node in assetGroups.Keys) {
			var result = assetGroups[node];
			foreach (var groupKey in result.Keys) {
				var assets = result[groupKey];

				sb.AppendFormat("In {0}:\n", groupKey);

				foreach (var a in assets) {					
					sb.AppendFormat("\t {0} {1}\n", a.path, (a.isBundled)?"[in AssetBundle]":"");
				}
			}
			sb.Append("\n");
		}

		/*
			このあたりに自前でコードを書くことで、
			
			ビルド -> 完了した結果を外部のサービスへと通知
			というようなことができる。

			で、ここでは架空のメールアドレスにBuild結果を投稿してみる。
			ちなみに下記のコードでgmailとかに投げるとすごく怒られるので各位工夫してね。参考までに、hotmailとか意識がダメそうなやつは通った。
		*/

		var smtpClientHost = "VVV.WWW.com";//　smtp.live.comとか
		var mailAddress = "XXXXXXX@YYYYYYY.com";
		var password = "ZZZZZZ";
		var body = sb.ToString();

		MailMessage mail = new MailMessage();

		mail.From = new MailAddress(mailAddress);
		mail.To.Add(mailAddress);
		mail.Subject = "AssetBundleGraph build is over.";
		mail.Body = body;

		SmtpClient smtpServer = new SmtpClient(smtpClientHost);
		smtpServer.Port = 587;
		smtpServer.Credentials = new System.Net.NetworkCredential(mailAddress, password) as ICredentialsByHost;
		smtpServer.EnableSsl = true;
		ServicePointManager.ServerCertificateValidationCallback = 
		delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
			return true;
		};
		smtpServer.Send(mail);

		/*
			今回は適当なメールを送るのみにしたけれど、例えば全てのAssetに対してimporterFromをキーに整理すると、
			とあるpathにあるAssetがどんなNodeを通過したのか、などが把握できる。
		*/
	}
}
