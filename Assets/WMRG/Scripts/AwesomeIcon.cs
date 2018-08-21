//This script shows fontawesome icons
//Taken from http://forum.unity3d.com/threads/image-fonts-fontawesome.281746/

//To use it you need to put this script on empty object, put "fontawesome-webfont" as font and fill "Text field" with icon code like \uf***
//All icons sheet with codes - http://fortawesome.github.io/Font-Awesome/cheatsheet/
//Example: if code shown on sheet is &#xf0a2 you should put xf0a2 (without '&#')

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;

namespace UnityEngine.UI {
	public class AwesomeIcon : Text {
		private bool disableDirty = false;
		private Regex regexp = new Regex(@"\\u(?<Value>[a-zA-Z0-9]{4})");
		 
		#if UNITY_5_2_1 || UNITY_5_2_0
		protected override void OnPopulateMesh(Mesh toFill) {
			string cache = this.text;
			string replaced = cache.Replace("x", "\\u");
			disableDirty = true;
			this.text = this.Decode(replaced);
			base.OnPopulateMesh(toFill);
			this.text = cache;
			disableDirty = false;
		}
		
		#elif UNITY_5_0 || UNITY_5_1
		protected override void OnFillVBO(List<UIVertex> vbo) {
			string cache = this.text;
			string replaced = cache.Replace("x", "\\u"); 
			disableDirty = true;
			this.text = this.Decode(replaced);	 
			base.OnFillVBO(vbo);	 
			this.text = cache;	 
			disableDirty = false;
		}

		#else
		protected override void OnPopulateMesh(VertexHelper toFill) {
			string cache = this.text;
			string replaced = cache.Replace("x", "\\u");
			disableDirty = true;
			this.text = this.Decode(replaced);
			base.OnPopulateMesh(toFill);
			this.text = cache;
			disableDirty = false;
		}
		#endif 
		  
		  
		private string Decode(string value) {
			return regexp.Replace(value, m => ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString());
		}
		  
		public override void SetLayoutDirty() {
			if (disableDirty) {
				return;
			}
			base.SetLayoutDirty();
		}
		  
		public override void SetVerticesDirty() {
			if (disableDirty) {
				return;
			}
			base.SetVerticesDirty();
		}
		  
		public override void SetMaterialDirty() {
			if (disableDirty) {
				return;
			}
			base.SetMaterialDirty();
		}
	}
}