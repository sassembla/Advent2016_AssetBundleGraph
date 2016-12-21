using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;
using System.Linq;

using AssetBundleGraph;

/*
	このコードは、AssetBundleGraphが生成したコードを、用途に合わせて編集している。
	AssetBundleGraphのグラフ中の、PrefabBuilder0という黄色いノードにセットされている。

	AssetBundleGraphをRefreshしたりBuildしたりすると、このコードの特定の関数が実行される。

	で、
	今回はAssetBundleGraphが出力したこのクラスを、適当に書き換えていくことにする。

	キューブを用意して、「複数枚のテクスチャをセットできるScript」をセットし、
	Scriptに対して複数枚のテクスチャをセットした状態にし、

	そのキューブをPrefabとして出力するのを書いてみた。


	ここまで複雑だとこのようにPrefabを作るためのコードが必須になっちゃうのが辛いところ。
*/
[AssetBundleGraph.CustomPrefabBuilder("MyBuilder")]
public class MyPrefabBuilder : IPrefabBuilder {

	/**
		 * Test if prefab can be created with incoming assets.
		 * @result Name of prefab file if prefab can be created. null if not.
		 */
	public string CanCreatePrefab (string groupKey, List<UnityEngine.Object> objects) {

		/*
			この関数では、セッティングの途中で「どんなオブジェクトがAssetBundleGraphから流れてくるか」というのをチェックしたりすることができる。
			ここでは各グループで6つずつのTextureが来るはずなので、
			
			6枚テクスチャがあったらPrefabができる という予定で値を返してみる。
		*/
		if (objects.Count != 6) {
			// 足りない！
			Debug.LogError("texture shortage. groupKey:" + groupKey);
			return null;
		}

		foreach (var obj in objects) {
			if (obj.GetType() != typeof(Texture2D)) {
				// テクスチャ2Dじゃない！！
				Debug.LogError("obj is not Texture2D. obj:" + obj + " in groupKey:" + groupKey);
				return null;
			}
		}

		// 入力されてるアセット的にOKだったら、最終的に作るつもりのPrefabの名前を返す。
		// ここでは、prefab_kohaku とかそういうのを想定している。
		return "prefab_" + groupKey;
	}

	/**
	 * Create Prefab.
	 */ 
	public UnityEngine.GameObject CreatePrefab (string groupKey, List<UnityEngine.Object> objects) {

		// objectsの中身はTexture2Dなので、UnityEngine.ObjectからTexture2Dに変換する。
		var textures = new Texture2D[6];
		for (var i = 0; i < objects.Count; i++) {
			var obj = objects[i];
			textures[i] = obj as Texture2D;
		}

		// GameObjectを作成する。ここではCubeが好きなのでCubeを作る。
		GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);

		// 今回Texture2Dを６枚セットする対象のスクリプトを、goにくっつける。
		MyCubeScript myCubeScr = go.AddComponent<MyCubeScript>();

		// myCubeScrのtextures(Texture2DのArray)に、ダイレクトにセット。
		myCubeScr.textures = textures;
		
		/*
			最終的にGameObjectを返すと、そのGameObjectが自動的にPrefabになる！ 
			わあPrefabできちゃったよ。
			名前はCanCreatePrefab関数で返したものが使われる。
		*/
		return go;

		// そしてなんと、このGameObjectはこのあと自動的に消去される。
	}

	/**
	 * Draw Inspector GUI for this PrefabBuilder.
	 */ 
	public void OnInspectorGUI (Action onValueChanged) {

		/*
			このPrefabBuilderのために、インスペクタを書くことができる。
			ちゃんと作るとGUIから一連の設定をすることも夢じゃないぞ！
			でもサボってもいいと思う。
		*/
		
	}

	/**
	 * Serialize this PrefabBuilder to JSON using JsonUtility.
	 */ 
	public string Serialize() {

		/*
			シリアライザでこのインスタンスをjsonにすることで、このクラスにもたせた設定を永続化させることができる。
			なぜこんなことしているか具体的に言うと、
			
			このクラスになんらか[SerializeField]な変数を定義する ->
			変数の内容をOnInspectorGUIから調整可能にする or 直書きする ->
			OnInspectorGUIでonValueChangedを呼んだタイミングで、このクラスをまるごと設定としてSerializeする ->
			動作時にそのパラメータを設定から読み出し、prefab作成に際して適応させる

			という動作が起こる。そのうちもっと隠蔽されて綺麗になるんだと思う。
		*/ 

		return JsonUtility.ToJson(this);
	}
}
