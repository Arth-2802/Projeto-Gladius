using UnityEngine;

public class EfeitoEspada : MonoBehaviour
{
    private Sprite[] frames;
    private SpriteRenderer sr;
    private int direcao = 1;
    private float fps = 14f;
    private float velocidade = 3f;
    private float tempo;

    public void Iniciar(Sprite[] quadros, int dir, int sortingLayerId, int ordem)
    {
        frames = quadros;
        direcao = dir;

        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sortingLayerID = sortingLayerId;
        sr.sortingOrder = ordem;
        sr.flipX = dir < 0;
        sr.sprite = frames[0];
    }

    void Update()
    {
        if (frames == null) return;

        tempo += Time.deltaTime;
        int i = (int)(tempo * fps);

        if (i >= frames.Length)
        {
            Destroy(gameObject);
            return;
        }

        sr.sprite = frames[i];
        transform.position += new Vector3(direcao * velocidade * Time.deltaTime, 0f, 0f);
    }
}