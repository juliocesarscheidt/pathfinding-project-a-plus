using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

class CelulaComparer : IComparer<Celula> {
    public int Compare(Celula a, Celula b) {
        return a.custoTotal.CompareTo(b.custoTotal);
    }
}

public class AlgoritmoAStar : MonoBehaviour
{
    public static AlgoritmoAStar Instance { get; private set; }

    public GameObject componentePaiDasCelulas;
    public List<Celula> todasCelulas = new();
    public Celula celulaInicial;
    public Celula celulaFinal;
    private bool encontrouMelhorCaminho = false;
    private bool iniciado = false;
    public Button botaoIniciar;
    public Button botaoResetar;
    public TextMeshProUGUI textoCustoTotal;
    public Color corPadraoCelula;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
        }

        ResetarCelulas();
    }

    public void ResetarCelulas() {
        textoCustoTotal.text = "Custo total: ";
        encontrouMelhorCaminho = false;

        todasCelulas.RemoveAll(celula => celula != null);
        foreach (var celula in componentePaiDasCelulas.GetComponentsInChildren<Celula>()) {
            todasCelulas.Add(celula);

            if (celula.isInicio) celulaInicial = celula;
            if (celula.isFim) celulaFinal = celula;

            celula.SetarIsObstaculo(false);
        }

        celulaInicial.GetComponent<SpriteRenderer>().color = Color.red;
        celulaFinal.GetComponent<SpriteRenderer>().color = Color.red;

        CalcularDistancias();

        iniciado = false;
        botaoIniciar.enabled = true;
        botaoResetar.enabled = true;
    }

    public void CalcularDistancias() {
        // calcular as distancias entre as celulas
        List<Celula> celulasVisitadas = new();
        Queue<Celula> celulasParaVisitar = new();

        celulasParaVisitar.Enqueue(celulaFinal);

        while (celulasParaVisitar.Count > 0) {
            Celula celulaAtual = celulasParaVisitar.Dequeue();
            // celulaAtual.GetComponent<SpriteRenderer>().color = Color.gray;

            foreach (var vizinho in celulaAtual.vizinhos) {
                if (celulasVisitadas.Contains(vizinho)) {
                    continue;
                }
                // vizinho.GetComponent<SpriteRenderer>().color = Color.cyan;

                vizinho.custoTotal = 0;
                vizinho.celulaPai = null;
                vizinho.custoAcumulado = 0;

                if (vizinho.isObstaculo) {
                    vizinho.custoRestante = 0;
                } else {
                    vizinho.custoRestante = celulaAtual.custoRestante + 1;
                    celulasParaVisitar.Enqueue(vizinho);
                }
            }
            celulasVisitadas.Add(celulaAtual);
        }
    }

    public void CalcularCaminho() {
        if (iniciado) {
            return;
        }

        CalcularDistancias();

        textoCustoTotal.text = "Custo total: ";
        encontrouMelhorCaminho = false;

        iniciado = true;
        botaoIniciar.enabled = false;
        botaoResetar.enabled = false;

        StartCoroutine(CalcularCaminhoEnumerator());
    }

    public IEnumerator CalcularCaminhoEnumerator() {
        List<Celula> listaAberta = new();
        List<Celula> listaFechada = new();

        listaAberta.Add(celulaInicial);
        
        while (listaAberta.Count > 0) {
            Celula celulaAtual = listaAberta[0];

            if (celulaAtual != celulaInicial && celulaAtual != celulaFinal) {
                celulaAtual.GetComponent<SpriteRenderer>().color = Color.green;
            }

            if (celulaAtual.isObstaculo) {
                listaAberta.Remove(celulaAtual);
                continue;
            }

            // calcula o custo total da celula atual
            celulaAtual.custoTotal = celulaAtual.custoAcumulado + celulaAtual.custoRestante;

            // Adiciona os vizinhos da celula inicial na lista aberta
            foreach (var vizinho in celulaAtual.vizinhos) {
                if (!listaFechada.Contains(vizinho) && !vizinho.isObstaculo) {
                    if (vizinho != celulaInicial && vizinho != celulaFinal) {
                        vizinho.GetComponent<SpriteRenderer>().color = Color.cyan;
                    }

                    // calcula o custo acumulado do vizinho se ele ainda não foi visitado
                    if (!listaAberta.Contains(vizinho)) {
                        vizinho.celulaPai = celulaAtual;
                        vizinho.custoAcumulado = vizinho.celulaPai.custoAcumulado + 1;
                    }

                    listaAberta.Add(vizinho);
                }
            }

            listaFechada.Add(celulaAtual);

            listaAberta.Remove(celulaAtual);
            // ordena a lista aberta pelo custo total decrescente
            listaAberta.Sort(new CelulaComparer());

            if (celulaAtual == celulaFinal) {
                encontrouMelhorCaminho = true;
                break;
            }

            yield return StartCoroutine(Sleep(0.05f));
        }

        if (encontrouMelhorCaminho) {
            MostrarMelhorCaminho();
        }

        listaAberta.Clear();
        listaFechada.Clear();

        iniciado = false;
        botaoIniciar.enabled = false;
        botaoResetar.enabled = true;
    }

    public void MostrarMelhorCaminho() {
        List<Celula> celulasMelhorCaminho = new();

        // encontra o caminho retornando pelos pai da celula atual
        Celula celulaPonteiro = celulaFinal;
        while (celulaPonteiro != null) {
            if (celulaPonteiro.isObstaculo) {
                break;
            }
            celulasMelhorCaminho.Add(celulaPonteiro);
            celulaPonteiro = celulaPonteiro.celulaPai;
        }
        foreach (var celula in celulasMelhorCaminho) {
            celula.GetComponent<SpriteRenderer>().color = Color.gray;
        }
        textoCustoTotal.text = $"Custo total: {celulaFinal.custoTotal}";

        celulasMelhorCaminho.Clear();
    }

    void Update() {
        if (!iniciado && Input.GetMouseButtonDown(0)) {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity);

            if (hit.collider != null) {
                // Debug.Log($"hit.collider {hit.collider}");
                if (hit.collider.CompareTag("Celula") && hit.collider.TryGetComponent(out Celula celula)) {
                    if (celula.isInicio || celula.isFim) {
                        return;
                    }
                    celula.SetarIsObstaculo(!celula.isObstaculo);
                }
            }
        }
    }

    IEnumerator Sleep(float seconds) {
        yield return new WaitForSeconds(seconds);
    }
}
