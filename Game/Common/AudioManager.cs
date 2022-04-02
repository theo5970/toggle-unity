using System.Collections.Generic;
using Toggle.Utils;
using UnityEngine;

namespace Toggle.Game.Common
{
    public class AudioManager : Singleton<AudioManager>
    {
        public AudioClip[] sfxList;
        public AudioClip[] musicList;

        private Dictionary<string, AudioClip> sfxContainer;

        public List<GameObject> sfxPool;

        // Start is called before the first frame update
        void Start()
        {
            sfxContainer = new Dictionary<string, AudioClip>();

            for (int i = 0; i < sfxList.Length; i++)
            {
                AudioClip sound = sfxList[i];
                sfxContainer.Add(sound.name, sound);
            }

            sfxPool = new List<GameObject>();
            for (int i = 0; i < 16; i++)
            {
                GameObject sfxObject = new GameObject();
                sfxObject.transform.SetParent(transform);
                sfxObject.AddComponent<SFXObject>();
                sfxObject.SetActive(false);

                sfxPool.Add(sfxObject);
            }
        }

        public GameObject GetPooledSFX()
        {
            int count = sfxPool.Count;
            for (int i = 0; i < count; i++)
            {
                GameObject obj = sfxPool[i];
                if (!obj.activeInHierarchy)
                {
                    return obj;
                }
            }

            return null;
        }

        public void PlaySFX(string name, float volume, float pitch)
        {
            AudioClip clip = sfxContainer[name];

            GameObject pooledObj = GetPooledSFX();
            pooledObj.SetActive(true);
            pooledObj.GetComponent<SFXObject>().Play(clip, volume, pitch);
        }
    }
}