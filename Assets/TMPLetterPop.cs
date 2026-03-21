using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TMPLetterPop : MonoBehaviour
{
	[Header("Pop Settings")]
	public float popDuration = 0.12f;
	public float settleDuration = 0.10f;
	public float jumpHeight = 18f;
	public float stretchX = 1.15f;
	public float stretchY = 0.85f;

	private TMP_Text _tmp;
	private int _lastVisibleCount = 0;

	private class CharAnim
	{
		public int charIndex;
		public float time;
		public bool settling;
	}

	private readonly List<CharAnim> _activeAnims = new List<CharAnim>();

	void Awake()
	{
		_tmp = GetComponent<TMP_Text>();
	}

	void LateUpdate()
	{
		_tmp.ForceMeshUpdate();

		int visibleCount = _tmp.maxVisibleCharacters;

		// detect newly revealed characters
		if (visibleCount > _lastVisibleCount)
		{
			for (int i = _lastVisibleCount; i < visibleCount; i++)
			{
				if (i < _tmp.textInfo.characterCount)
				{
					TMP_CharacterInfo charInfo = _tmp.textInfo.characterInfo[i];
					if (charInfo.isVisible)
					{
						_activeAnims.Add(new CharAnim
						{
							charIndex = i,
							time = 0f,
							settling = false
						});
					}
				}
			}
		}

		_lastVisibleCount = visibleCount;

		if (_activeAnims.Count > 0)
		{
			AnimateCharacters();
		}
	}

	void AnimateCharacters()
	{
		_tmp.ForceMeshUpdate();
		TMP_TextInfo textInfo = _tmp.textInfo;

		// Reset mesh data from original each frame before applying current animation
		for (int m = 0; m < textInfo.meshInfo.Length; m++)
		{
			textInfo.meshInfo[m].mesh.vertices = textInfo.meshInfo[m].vertices;
		}

		for (int a = _activeAnims.Count - 1; a >= 0; a--)
		{
			CharAnim anim = _activeAnims[a];

			if (anim.charIndex >= textInfo.characterCount)
			{
				_activeAnims.RemoveAt(a);
				continue;
			}

			TMP_CharacterInfo charInfo = textInfo.characterInfo[anim.charIndex];
			if (!charInfo.isVisible)
			{
				_activeAnims.RemoveAt(a);
				continue;
			}

			int materialIndex = charInfo.materialReferenceIndex;
			int vertexIndex = charInfo.vertexIndex;
			Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

			Vector3 bl = vertices[vertexIndex + 0];
			Vector3 tl = vertices[vertexIndex + 1];
			Vector3 tr = vertices[vertexIndex + 2];
			Vector3 br = vertices[vertexIndex + 3];

			Vector3 center = (bl + tr) / 2f;

			float totalDuration = popDuration + settleDuration;
			anim.time += Time.deltaTime;

			float t = Mathf.Clamp01(anim.time / totalDuration);

			// 0..1 over two phases
			float scaleX = 1f;
			float scaleY = 1f;
			float offsetY = 0f;

			if (anim.time <= popDuration)
			{
				float p = anim.time / popDuration;
				// fast outward pop
				scaleX = Mathf.Lerp(1f, stretchX, p);
				scaleY = Mathf.Lerp(1f, stretchY, p);
				offsetY = Mathf.Lerp(0f, jumpHeight, p);
			}
			else
			{
				float p = (anim.time - popDuration) / settleDuration;
				// settle back
				scaleX = Mathf.Lerp(stretchX, 1f, p);
				scaleY = Mathf.Lerp(stretchY, 1f, p);
				offsetY = Mathf.Lerp(jumpHeight, 0f, p);
			}

			Matrix4x4 matrix =
				Matrix4x4.TRS(new Vector3(0f, offsetY, 0f), Quaternion.identity, new Vector3(scaleX, scaleY, 1f));

			vertices[vertexIndex + 0] = center + matrix.MultiplyPoint3x4(bl - center);
			vertices[vertexIndex + 1] = center + matrix.MultiplyPoint3x4(tl - center);
			vertices[vertexIndex + 2] = center + matrix.MultiplyPoint3x4(tr - center);
			vertices[vertexIndex + 3] = center + matrix.MultiplyPoint3x4(br - center);

			if (anim.time >= totalDuration)
			{
				_activeAnims.RemoveAt(a);
			}
		}

		for (int m = 0; m < textInfo.meshInfo.Length; m++)
		{
			textInfo.meshInfo[m].mesh.vertices = textInfo.meshInfo[m].vertices;
			_tmp.UpdateGeometry(textInfo.meshInfo[m].mesh, m);
		}
	}

	public void ResetTracking()
	{
		_lastVisibleCount = 0;
		_activeAnims.Clear();
	}
}