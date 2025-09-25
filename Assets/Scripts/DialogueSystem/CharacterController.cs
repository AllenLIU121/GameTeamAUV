using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace DialogueSystem
{
    /// <summary>
    /// 角色控制器（负责角色显示、隐藏、淡入淡出）
    /// </summary>
    public class CharacterController
    {
        private readonly List<CharacterData> _characters;
        private readonly float _fadeTime;
        private string _currentCharacterName = "";

        public CharacterController(List<CharacterData> characters, float fadeTime)
        {
            _characters = characters;
            _fadeTime = fadeTime;
            InitializeCharacters();
        }

        /// <summary>
        /// 初始化所有角色（默认隐藏）
        /// </summary>
        private void InitializeCharacters()
        {
            foreach (var character in _characters)
            {
                if (character.characterSprite != null)
                {
                    character.characterSprite.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 切换角色（先隐藏当前角色，再显示新角色）
        /// </summary>
        public IEnumerator ChangeCharacter(string newCharacterName, System.Action onComplete)
        {
            // 角色不变，直接回调
            if (_currentCharacterName == newCharacterName)
            {
                onComplete?.Invoke();
                yield break;
            }

            // 隐藏当前角色
            if (!string.IsNullOrEmpty(_currentCharacterName))
            {
                var currentChar = GetCharacterData(_currentCharacterName);
                if (currentChar != null && currentChar.characterSprite != null)
                {
                    yield return FadeCharacter(currentChar.characterSprite, false);
                    currentChar.characterSprite.SetActive(false);
                }
            }

            // 显示新角色
            var newChar = GetCharacterData(newCharacterName);
            if (newChar != null && newChar.characterSprite != null)
            {
                newChar.characterSprite.SetActive(true);
                newChar.characterSprite.transform.localPosition = newChar.characterPosition;
                yield return FadeCharacter(newChar.characterSprite, true);
            }

            // 更新当前角色名称并回调
            _currentCharacterName = newCharacterName;
            onComplete?.Invoke();
        }

        /// <summary>
        /// 隐藏当前角色（对话结束时调用）
        /// </summary>
        public IEnumerator HideCurrentCharacter()
        {
            if (!string.IsNullOrEmpty(_currentCharacterName))
            {
                var currentChar = GetCharacterData(_currentCharacterName);
                if (currentChar != null && currentChar.characterSprite != null)
                {
                    yield return FadeCharacter(currentChar.characterSprite, false);
                    currentChar.characterSprite.SetActive(false);
                }
                _currentCharacterName = "";
            }
        }

        /// <summary>
        /// 角色淡入淡出动画
        /// </summary>
        private IEnumerator FadeCharacter(GameObject character, bool fadeIn)
        {
            // 优先处理Image组件（UI角色），其次处理SpriteRenderer（世界角色）
            var image = character.GetComponent<Image>();
            if (image != null)
            {
                yield return FadeImage(image, fadeIn);
                yield break;
            }

            var spriteRenderer = character.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                yield return FadeSpriteRenderer(spriteRenderer, fadeIn);
            }
        }

        /// <summary>
        /// Image组件淡入淡出
        /// </summary>
        private IEnumerator FadeImage(Image image, bool fadeIn)
        {
            var startAlpha = fadeIn ? 0f : 1f;
            var targetAlpha = fadeIn ? 1f : 0f;
            var elapsedTime = 0f;
            var color = image.color;
            color.a = startAlpha;
            image.color = color;

            while (elapsedTime < _fadeTime)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / _fadeTime);
                image.color = color;
                yield return null;
            }

            color.a = targetAlpha;
            image.color = color;
        }

        /// <summary>
        /// SpriteRenderer组件淡入淡出
        /// </summary>
        private IEnumerator FadeSpriteRenderer(SpriteRenderer renderer, bool fadeIn)
        {
            var startAlpha = fadeIn ? 0f : 1f;
            var targetAlpha = fadeIn ? 1f : 0f;
            var elapsedTime = 0f;
            var color = renderer.color;
            color.a = startAlpha;
            renderer.color = color;

            while (elapsedTime < _fadeTime)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / _fadeTime);
                renderer.color = color;
                yield return null;
            }

            color.a = targetAlpha;
            renderer.color = color;
        }

        /// <summary>
        /// 根据名称获取角色数据
        /// </summary>
        private CharacterData GetCharacterData(string characterName)
        {
            return _characters.Find(c => c.characterName == characterName);
        }
    }
}