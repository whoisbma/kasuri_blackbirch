using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Kasuri : MonoBehaviour {
  private Cell[,] cells;
  public Cell cellPrefab;
  public Kasuri childKasuriPrefab;
  public int[] activeRows;
  public Kasuri[,] children;
  public Transform childParent;

  public bool isChild = false;
  public int childrenX = 0;
  public int childrenY = 0;
  public int wStart = 7;
  public int hStart = 7;
  public int numActive = 4;
  public float maxSize;
  public float cellSeparation = 1.1f;

  private int hActual;
  private Transform gridBG;

  private void Awake() {
    gridBG = transform.Find("Grid BG");    
  }

  private void Start() {
    if (!isChild) { 
      Init(wStart, hStart);
    }
  }

  public void ChangeNumActive(Slider slider) {
    if ((int)slider.value == numActive) return;
    if ((int)slider.value < cells.GetLength(1)) {
      Reset();
      numActive = (int)slider.value;
      Init(cells.GetLength(0), cells.GetLength(1));
    } else {
      slider.value = cells.GetLength(1) - 1;
    }

  }

  public void ChangeDimX(Slider slider) {
    if ((int)slider.value == cells.GetLength(0)) return;
    Reset();
    Init((int)slider.value, cells.GetLength(1));
  }

  public void ChangeDimY(Slider slider) {
    if ((int)slider.value == cells.GetLength(1)) return;
    if ((int)slider.value < numActive + 1) {
      slider.value = cells.GetLength(1);
      return;
    }
    Reset();
    Init(cells.GetLength(0), (int)slider.value);
  }

  public void ChangeActiveNum(Slider slider) {
    Reset();
    numActive = (int)slider.value;
    Init(cells.GetLength(0), cells.GetLength(1));
  }

  public void Restart() {
    numActive = 4;
    Reset();
    Init(wStart, hStart);
  }

  public void Randomize() {
    for (int i = 0; i < activeRows.Length; i++) {
      UpdateColumn(i, Mathf.FloorToInt(Random.Range(0, cells.GetLength(1))));
    }
  }

  public void DestroyCells() {
    for (int i = 0; i < cells.GetLength(0); i++) {
      activeRows[i] = 0;
      for (int j = 0; j < cells.GetLength(1); j++) {
        Destroy(cells[i, j].gameObject);
      }
    }
  }

  public void DestroyChildren() {
    for (int i = 0; i < childrenX; i++) {
      for (int j = 0; j < childrenY; j++) {
        children[i, j].DestroyCells();
        Destroy(children[i, j].gameObject);
      }
    }
  }

  private void Init(int w, int h) {
    children = new Kasuri[childrenX, childrenY];
    cells = new Cell[w, h];
    activeRows = new int[w];
    hActual = h - numActive + 1;
    float sizeModX = cellPrefab.GetComponent<SpriteRenderer>().sprite.bounds.size.x * transform.localScale.x;
    float sizeModY = cellPrefab.GetComponent<SpriteRenderer>().sprite.bounds.size.y * transform.localScale.y;

    float currentSizeInLargestDim = Mathf.Max(Mathf.Abs(sizeModX * cells.GetLength(0)), Mathf.Abs(sizeModY * cells.GetLength(1)));
    float proportionDiff = maxSize / currentSizeInLargestDim;

    float cellSizeModifier = 1;
    if (!isChild) {
      sizeModX *= proportionDiff * cellSeparation;
      sizeModY *= proportionDiff * cellSeparation;
      cellSizeModifier *= proportionDiff;
    }


    CreateCells(cellSizeModifier, sizeModX, sizeModY);
    if (!isChild) {
      CreateChildren();
    }
  }

  private void CreateCells(float cellSizeModifier, float sizeModX, float sizeModY) {
    Vector3 pos;
    for (int i = 0; i < cells.GetLength(0); i++) {
      for (int j = 0; j < cells.GetLength(1); j++) {
        pos = new Vector3(
          transform.position.x + (i - cells.GetLength(0) * 0.5f) * sizeModX + sizeModX / 2,
          transform.position.y + (j - cells.GetLength(1) * 0.5f) * sizeModY + sizeModY / 2,
          0);
        cells[i, j] = Instantiate<Cell>(cellPrefab, pos, Quaternion.identity, transform);
        cells[i, j].Init(i, j, cellSizeModifier, j < activeRows[i] + numActive);
      }
    }

    if (gridBG) {
      gridBG.localScale = new Vector3(sizeModX * cells.GetLength(0) * 2 + cellSeparation / 2f, sizeModY * cells.GetLength(1) * 2 + cellSeparation / 2f, 1);
    }

  }

  private void CreateChildren() {
    Vector3 pos;
    float scale = 1.0f;
    float separationX = 2.7f * 7f / cells.GetLength(0);
    float separationY = 2.7f * 7f / cells.GetLength(1);
    float sizeModX = (cellPrefab.GetComponent<SpriteRenderer>().sprite.bounds.size.x * cells.GetLength(0)) * scale;
    float sizeModY = (cellPrefab.GetComponent<SpriteRenderer>().sprite.bounds.size.y * cells.GetLength(1)) * scale;

    for (int i = 0; i < childrenX; i++) {
      for (int j = 0; j < childrenY; j++) {
        children[i, j] = Instantiate(childKasuriPrefab, childParent.localPosition, Quaternion.identity, childParent);
        children[i, j].transform.localScale = new Vector3(scale, scale, 1);
        pos = new Vector3(
          ((i - children.GetLength(0) * 0.5f) * sizeModX + sizeModX/2f) * separationX,
          ((j - children.GetLength(1) * 0.5f) * sizeModY + sizeModY/2f) * separationY,
          0);
        children[i, j].transform.localPosition += pos;
        children[i, j].childrenX = 0;
        children[i, j].childrenY = 0;
        children[i, j].numActive = numActive;
        children[i, j].cellSeparation = 1;
        children[i, j].Init(cells.GetLength(0), cells.GetLength(1));
      }
    }
  }

  public void Reset() {
    try {
      DestroyCells();
    } catch {
      Debug.Log("no cells to destroy");

    };
    try {
      DestroyChildren();
    } catch {
      Debug.Log("no children to destroy");
    }
  }

  public void UpdateColumn(int x, int y) {
    if (cells[x, y].Active) return;

    int selectedRow = y - (cells.GetLength(1) - hActual);
    if (selectedRow > activeRows[x]) {
      activeRows[x] = selectedRow;
    } else {
      activeRows[x] = selectedRow + numActive - 1;
    }

    for (int i = 0; i < cells.GetLength(1); i++) {
      cells[x, i].Active = false;
    }

    for (int i = activeRows[x]; i < activeRows[x] + numActive; i++) {
      cells[x, i].Active = true;
    }

    for (int i = 0; i < childrenX; i++) {
      for (int j = 0; j < childrenY; j++) {
        children[i, j].UpdateColumn(x, y);        
      }
    }
  }

}