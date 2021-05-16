using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Generator : MonoBehaviour
{
    public int wysokosc;
    public int szerokosc;
    public int minRozmiar;
    public int maksRozmiar;
    public int procentPokoju;
    private readonly List<Rect> wezly = new List<Rect>();
    private readonly List<bool> czyLisc = new List<bool>();
    private readonly List<Rect> pokoje = new List<Rect>();
    private readonly List<Vector2> pary = new List<Vector2>();
    private readonly List<Rect> korytarze = new List<Rect>();
    private GameObject[,] plansza;
    public GameObject mPodloga;
    public GameObject mSciana;
    public GameObject mKorytarz;
    public GameObject mElementy;
    public GameObject ui;
    public GameObject wysokoscIn;
    public GameObject szerokoscIn;
    public GameObject minIn;
    public GameObject maksIn;
    public GameObject procentIn;
    public GameObject blad;
    public Camera kamera;

    //tworzenie diagramu

    public class Poziom
    {
        public Poziom lewy;
        public Poziom prawy;
        public Rect prostokat;

        public Poziom(Rect prostokat2)
        {
            prostokat = prostokat2;
        }

        //sprawdzanie czy wierzchołek jest liściem

        public bool Lisc()
        {
            if (lewy == null && prawy == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // dzielenie prostokątów na mniejsze części

        public bool Podziel(int minRozmiar)
        {
            if (Mathf.Min(prostokat.height, prostokat.width) / 2 < minRozmiar)
            {
                return false;
            }
            bool podzielPoz;
            if (prostokat.width / prostokat.height >= 1.25)
            {
                podzielPoz = false;
            } 
            else if (prostokat.height / prostokat.width >= 1.25)
            {
                podzielPoz = true;
            } 
            else
            {
                podzielPoz = Random.Range(0.0f, 1.0f) > 0.5;
            }
            if (podzielPoz == true)
            {
                int podziel = Random.Range(minRozmiar, (int)(prostokat.width - minRozmiar));
                lewy = new Poziom(new Rect(prostokat.x, prostokat.y, prostokat.width, podziel));
                prawy = new Poziom(new Rect(prostokat.x, (prostokat.y + podziel), 
                    prostokat.width, (prostokat.height - podziel)));
            } 
            else
            {
                int podziel = Random.Range(minRozmiar, (int)(prostokat.height - minRozmiar));
                lewy = new Poziom(new Rect(prostokat.x, prostokat.y, podziel, prostokat.height));
                prawy = new Poziom(new Rect((prostokat.x + podziel), 
                    prostokat.y, (prostokat.width - podziel), prostokat.height));
            }
            return true;
        }

        //sprawdzanie sąsiedztwa

        public Rect Sasiad()
        {
            if (Lisc())
            {
                return prostokat;
            }
            Rect prost;
            if (lewy != null)
            {
                prost = lewy.Sasiad();
                return prost;
            }
            if (prawy != null)
            {
                prost = prawy.Sasiad();
                return prost;

            }
            return new Rect(-1, -1, 0, 0);
        }
    }

    //tworzenie drzewa

    private void StworzDrzewo(Poziom poziom)
    {
        if (poziom.Lisc())
        {
            if ((poziom.prostokat.width > maksRozmiar
                || poziom.prostokat.height > maksRozmiar
                || Random.Range(0.0f, 1.0f) > 0.2))
            {
                if (poziom.Podziel(minRozmiar))
                {
                    wezly.Add(poziom.prostokat);
                    czyLisc.Add(false);
                    StworzDrzewo(poziom.lewy);
                    StworzDrzewo(poziom.prawy);
                    Pary(poziom.lewy, poziom.prawy);
                }
                else
                {
                    wezly.Add(poziom.prostokat);
                    czyLisc.Add(true);
                }
            }
            else
            {
                wezly.Add(poziom.prostokat);
                czyLisc.Add(true);
            }
        }
    }

    //tworzenie pokoi w liściach

    public void StworzPokoj(int procentPokoju)
    {
        for (int i = 0; i < wezly.Count; i++)
        {
            if (czyLisc[i] == true)
            {
                int szerokoscPokoju;
                int wysokoscPokoju;
                if (wezly[i].width - 3 > 0)
                {
                    szerokoscPokoju = (int)Random.Range(wezly[i].width * procentPokoju / 100, wezly[i].width - 3);
                }
                else
                {
                    szerokoscPokoju = 1;
                }
                if (wezly[i].height - 3 > 0)
                {
                    wysokoscPokoju = (int)Random.Range(wezly[i].height * procentPokoju / 100, wezly[i].height - 3);
                }
                else
                {
                    wysokoscPokoju = 1;
                }
                int polozenieX = (int)Random.Range(1, wezly[i].width - szerokoscPokoju - 1);
                int polozenieY = (int)Random.Range(1, wezly[i].height - wysokoscPokoju - 1);
                pokoje.Add(new Rect(wezly[i].x + polozenieX, wezly[i].y + polozenieY, szerokoscPokoju, wysokoscPokoju));
            }
            else
            {
                pokoje.Add(new Rect(0, 0, 0, 0));
            }
        }
    }

    //generowanie finalnej wielkości pokoi oraz tworzenie ich wizualnie

    public void NarysujPokoj()
    {
        int ktory = 0;
        int ile = 0;
        int finalny;
        int pierwszy;
        for (int b = 0; b < czyLisc.Count; b++)
        {
            if (czyLisc[b] == true)
            {
                ile++;
            }
        }
        if (ile > 4)
        {
            pierwszy = Random.Range(0, 2);
            finalny = Random.Range(ile * 80 / 100, ile);
        }
        else
        {
            pierwszy = 0;
            finalny = ile - 1;
        }

        for (int a = 0; a < pokoje.Count; a++)
        {
            if (czyLisc[a] == true)
            {
                int nrPodlogi = Random.Range(1, 5);
                for (int i = (int)pokoje[a].x - 1; i <= pokoje[a].xMax; i++)
                {
                    for (int j = (int)pokoje[a].y - 1; j <= pokoje[a].yMax; j++)
                    {
                        if (i == (int)pokoje[a].x - 1 || i == pokoje[a].xMax || j == (int)pokoje[a].y - 1 || j == pokoje[a].yMax)
                        {
                            
                            GameObject instancja = Instantiate(Resources.Load<GameObject>("scianaPrefab"), 
                                new Vector3(i, j, 0f), Quaternion.identity) as GameObject;
                            instancja.transform.parent = mSciana.transform;
                            plansza[i, j] = instancja;
                            
                        }
                        else
                        {
                            
                            GameObject instancja = Instantiate(Resources.Load<GameObject>("podloga" + nrPodlogi + "Prefab"), 
                                new Vector3(i, j, 0f), Quaternion.identity) as GameObject;
                            instancja.transform.parent = mPodloga.transform;
                            plansza[i, j] = instancja;
                            
                        }
                    }
                }
                if (ktory == pierwszy)
                {
                    int x = Random.Range((int)pokoje[a].x, (int)pokoje[a].xMax);
                    int y = Random.Range((int)pokoje[a].y, (int)pokoje[a].yMax);
                    GameObject instancja = Instantiate(Resources.Load<GameObject>("poczatekPrefab"), 
                        new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
                    instancja.transform.parent = mElementy.transform;
                    Destroy(plansza[x, y]);
                    plansza[x, y] = instancja;
                }
                if (ktory == finalny)
                {
                    int x = Random.Range((int)pokoje[a].x, (int)pokoje[a].xMax);
                    int y = Random.Range((int)pokoje[a].y, (int)pokoje[a].yMax);
                    GameObject instancja = Instantiate(Resources.Load<GameObject>("koniecPrefab"), 
                        new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
                    instancja.transform.parent = mElementy.transform;
                    Destroy(plansza[x, y]);
                    plansza[x, y] = instancja;
                }
                ktory++;
            }
        }

    }

    //wypełnianie listy par

    public void Pary(Poziom lewy, Poziom prawy)
    {
        int lewyIndex = wezly.LastIndexOf(lewy.Sasiad());
        int prawyIndex = wezly.LastIndexOf(prawy.Sasiad());
        pary.Add(new Vector2(lewyIndex, prawyIndex));
    }

    //tworzenie korytarzy łączących pokoje

    public void StworzKorytarz()
    {
        for(int i = 0; i < pary.Count; i++)
        {
            Rect lewyP = pokoje[(int)pary[i].x];
            Rect prawyP = pokoje[(int)pary[i].y];
            Vector2 lp = new Vector2((int)Random.Range(lewyP.x, lewyP.xMax ), (int)Random.Range(lewyP.y , lewyP.yMax ));
            Vector2 rp = new Vector2((int)Random.Range(prawyP.x, prawyP.xMax ), (int)Random.Range(prawyP.y , prawyP.yMax ));
            if (lp.x == rp.x)
            {
                if (lp.y < rp.y)
                {
                    korytarze.Add(new Rect((int)lp.x, (int)lp.y, 1, (int)(rp.y - lp.y)));
                }
                else
                {
                    korytarze.Add(new Rect((int)rp.x, (int)rp.y, 1, (int)(lp.y - rp.y)));
                }
            }
            else if (lp.y == rp.y)
            {
                if (lp.x < rp.x)
                {
                    korytarze.Add(new Rect((int)lp.x, (int)lp.y, (int)(rp.x - lp.x), 1));
                }
                else
                {
                    korytarze.Add(new Rect((int)rp.x, (int)rp.y, (int)(lp.x - rp.x), 1));
                }
            }
            else
            {
                if (lp.y < rp.y)
                {
                    korytarze.Add(new Rect((int)lp.x, (int)lp.y, 1, (int)(rp.y - lp.y)));
                    if (lp.x < rp.x)
                    {
                        korytarze.Add(new Rect((int)lp.x, (int)rp.y, (int)(rp.x - lp.x)+1, 1));
                    }
                    else
                    {
                        korytarze.Add(new Rect((int)rp.x, (int)rp.y, (int)(lp.x - rp.x)+1, 1));
                    }
                }
                else
                {
                    korytarze.Add(new Rect((int)lp.x, (int)rp.y, 1, (int)(lp.y - rp.y)));
                    if (lp.x < rp.x)
                    {
                        korytarze.Add(new Rect((int)lp.x, (int)rp.y, (int)(rp.x - lp.x)+1, 1));
                    }
                    else
                    {
                        korytarze.Add(new Rect((int)rp.x, (int)rp.y, (int)(lp.x - rp.x)+1, 1));
                    }
                }
            }
        }
    }

    //wizualizacja korytarzy

    public void NarysujKorytarz()
    {
        for (int a = 0; a < korytarze.Count; a++)
        {
            for (int i = (int)korytarze[a].x - 1; i <= korytarze[a].xMax; i++)
            {
                for (int j = (int)korytarze[a].y - 1; j <= korytarze[a].yMax; j++)
                {
                    if (i == (int)korytarze[a].x - 1 || i == korytarze[a].xMax || j == (int)korytarze[a].y - 1 || j == korytarze[a].yMax)
                    {
                        if (plansza[i, j] == null)
                        {
                            
                            GameObject instancja = Instantiate(Resources.Load<GameObject>("scianaPrefab"), 
                                new Vector3(i, j, 0f), Quaternion.identity) as GameObject;
                            instancja.transform.parent = mSciana.transform;
                            plansza[i, j] = instancja;
                            
                        }
                    }
                    else
                    {
                        if (plansza[i, j] == null)
                        {
                            GameObject instancja = Instantiate(Resources.Load<GameObject>("korytarzPrefab"), 
                                new Vector3(i, j, 0f), Quaternion.identity) as GameObject;
                            instancja.transform.parent = mKorytarz.transform;
                            plansza[i, j] = instancja;
                        }
                        else if (plansza[i, j].name == "scianaPrefab(Clone)")
                        {
                            Destroy(plansza[i, j]);
                            GameObject instancja = Instantiate(Resources.Load<GameObject>("korytarzPrefab"), 
                                new Vector3(i, j, 0f), Quaternion.identity) as GameObject;
                            instancja.transform.parent = mKorytarz.transform;
                            plansza[i, j] = instancja;
                        }
                    }
                }
            }
        }
    }

    //skalowanie kamery

    private void UstawKamere()
    {
        kamera.transform.position = new Vector3(szerokosc / 2, wysokosc / 2, -10);
        kamera.orthographicSize = Mathf.Max(szerokosc, wysokosc) / 2 * 1.1f;
    }

    //sprawdzanie poprawności dancyh oraz wywoływanie poszczególnych funkcji

    private void Uruchom()
    {     
        wysokosc = int.Parse(wysokoscIn.GetComponent<InputField>().text);
        szerokosc = int.Parse(szerokoscIn.GetComponent<InputField>().text);
        minRozmiar = int.Parse(minIn.GetComponent<InputField>().text);
        maksRozmiar = int.Parse(maksIn.GetComponent<InputField>().text);
        procentPokoju = int.Parse(procentIn.GetComponent<InputField>().text);

        if ((procentPokoju * maksRozmiar / 100) >= maksRozmiar - 3 || (procentPokoju * minRozmiar / 100) < 1 
            || minRozmiar >= maksRozmiar || Mathf.Min(minRozmiar,maksRozmiar) > Mathf.Min(szerokosc, wysokosc))
        {
            blad.SetActive(true);
            Debug.Log("Wpisz poprawne parametry");
            return;
        }
        else
        {
            wezly.Clear();
            czyLisc.Clear();
            pokoje.Clear();
            korytarze.Clear();
            pary.Clear();

            if (plansza != null)
            {
                foreach (GameObject obj in plansza)
                {
                    Destroy(obj);
                }
            }
            ui.SetActive(false);
            blad.SetActive(false);
            UstawKamere();
            plansza = new GameObject[szerokosc + 1, wysokosc + 1];
            Poziom glownyPoziom = new Poziom(new Rect(0, 0, szerokosc, wysokosc));
            StworzDrzewo(glownyPoziom);
            StworzPokoj(procentPokoju);
            StworzKorytarz();
            NarysujPokoj();
            NarysujKorytarz();
        }
    }

    //sprawdzanie czy pola są wypełnione i uruchamianie

    public void Guzik()
    {
        if (wysokoscIn.GetComponent<InputField>().text == ""
            || szerokoscIn.GetComponent<InputField>().text == ""
            || minIn.GetComponent<InputField>().text == ""
            || maksIn.GetComponent<InputField>().text == ""
            || procentIn.GetComponent<InputField>().text == "")
        {
            blad.SetActive(true);
        }
        else
        {
            Uruchom();
        }
    }

    //wyświetlanie menu

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (ui.activeSelf == false)
            {
                ui.SetActive(true);
            }
            else
            {
                ui.SetActive(false);
            }

        }  
    }
}