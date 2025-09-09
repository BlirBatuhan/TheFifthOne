using System.Collections.Generic;
using UnityEngine;

namespace Kutuphanem
{
    public class MyLibrary
    {
        // Sol hareket animasyonu - art�k do�rudan input verisini al�yoruz
        public void Sol_Hareket(Animator anim, string AnaParatme,
               List<float> ParametreDegerleri, NetworkInputData inputData)
        {
            if (inputData.move.x < -0.1f) // sola hareket var m�
            {
                if (inputData.move.y > 0.1f) // W+A
                {
                    anim.SetFloat(AnaParatme, ParametreDegerleri[1]);
                }
                else if (inputData.move.y < -0.1f) // S+A
                {
                    anim.SetFloat(AnaParatme, ParametreDegerleri[2]);
                }
                else // sadece sola
                {
                    anim.SetFloat(AnaParatme, ParametreDegerleri[0]);
                }
            }
            else
            {
                anim.SetFloat(AnaParatme, 0);
            }
        }

        // Sa� hareket animasyonu
        public void Sag_Hareket(Animator anim, string AnaParatme,
             List<float> ParametreDegerleri, NetworkInputData inputData)
        {
            if (inputData.move.x > 0.1f) // sa�a hareket var m�
            {
                if (inputData.move.y > 0.1f) // W+D
                {
                    anim.SetFloat(AnaParatme, ParametreDegerleri[1]);
                }
                else if (inputData.move.y < -0.1f) // S+D
                {
                    anim.SetFloat(AnaParatme, ParametreDegerleri[2]);
                }
                else // sadece sa�a
                {
                    anim.SetFloat(AnaParatme, ParametreDegerleri[0]);
                }
            }
            else
            {
                anim.SetFloat(AnaParatme, 0);
            }
        }

        // Geri hareket animasyonu
        public void Geri_Hareket(Animator anim, string AnaParatme, NetworkInputData inputData)
        {
            if (inputData.move.y < -0.1f) // geri hareket var m�
            {
                anim.SetFloat("speed", 0);
                anim.SetBool(AnaParatme, true);
            }
            else
            {
                anim.SetBool(AnaParatme, false);
            }
        }

        // E�ilme animasyonu
        public void Egilme_Hareket(Animator anim, string AnaParatme,
             List<float> ParametreDegerleri, NetworkInputData inputData)
        {
            if (inputData.crouch) // e�ilme tu�u aktif mi
            {
                if (inputData.move.y > 0.1f)
                {
                    anim.SetFloat(AnaParatme, Mathf.Lerp(anim.GetFloat(AnaParatme), ParametreDegerleri[1], Time.deltaTime * 10f));
                }
                else if (inputData.move.y < -0.1f)
                {
                    anim.SetFloat(AnaParatme, Mathf.Lerp(anim.GetFloat(AnaParatme), ParametreDegerleri[2], Time.deltaTime * 10f));
                }
                else if (inputData.move.x < -0.1f)
                {
                    anim.SetFloat(AnaParatme, Mathf.Lerp(anim.GetFloat(AnaParatme), ParametreDegerleri[3], Time.deltaTime * 10f));
                }
                else if (inputData.move.x > 0.1f)
                {
                    anim.SetFloat(AnaParatme, Mathf.Lerp(anim.GetFloat(AnaParatme), ParametreDegerleri[4], Time.deltaTime * 10f));
                }
                else
                {
                    anim.SetFloat(AnaParatme, ParametreDegerleri[0]);
                }
            }
            else
            {
                anim.SetFloat(AnaParatme, 0);
            }
        }

        public List<float> ParamtereOlustur(float[] parametre)
        {
            List<float> Yon_Parametreleri = new List<float>();
            foreach (float item in parametre)
            {
                Yon_Parametreleri.Add(item);
            }
            return Yon_Parametreleri;
        }
    }
}
