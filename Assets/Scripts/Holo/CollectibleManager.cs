using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    private readonly int MAX_COLLECTIBLE = 6;
    [SerializeField] private int m_collectibleNumber;

    [SerializeField] private GameObject[] m_anchores;

    private void FixedUpdate() {
        UpdateCollectible();
    }

    private void UpdateCollectible()
    {
        for (int i = 0; i < MAX_COLLECTIBLE; i++)
        {
            if (i < m_collectibleNumber && !m_anchores[i].activeSelf)
            {
                m_anchores[i].SetActive(true);
            }
            else if (i >= m_collectibleNumber && m_anchores[i].activeSelf)
            {
                m_anchores[i].SetActive(false);
            }
        }
    }

}
