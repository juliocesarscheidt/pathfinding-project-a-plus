using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Celula : MonoBehaviour
{
    public int custoAcumulado = 0;
    public int custoRestante = 0;
    public int custoTotal = 9;
    public bool isObstaculo = false;
    public bool isInicio = false;
    public bool isFim = false;
    public Celula celulaPai;
    public List<Celula> vizinhos = new();

    void Start() {
        CalcularTodosVizinhos();
    }

    public void CalcularTodosVizinhos() {
        vizinhos.Clear();
        if (isObstaculo) {
            return;
        }
        foreach (var outraCelula in AlgoritmoAStar.Instance.todasCelulas) {
            if (outraCelula == this) {
                continue;
            }
            float distancia = Vector2.Distance(transform.position, outraCelula.transform.position);
            if (distancia == 1f) {
                // Debug.Log($"distancia do {nome} para {outraCelula.nome} :: {distancia}");
                vizinhos.Add(outraCelula.GetComponent<Celula>());
            }
        }
    }

    public void SetarIsObstaculo(bool isObstaculo) {
        this.isObstaculo = isObstaculo;
        if (isObstaculo) {
            GetComponent<SpriteRenderer>().color = Color.black;
        } else {
            GetComponent<SpriteRenderer>().color = AlgoritmoAStar.Instance.corPadraoCelula;
        }
    }
}
