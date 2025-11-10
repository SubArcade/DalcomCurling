using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public static class BoardSaveManager
{
    private static FirebaseFirestore db => FirebaseFirestore.DefaultInstance;

    // Firestore에 저장
    public static async Task SaveToFirestore(BoardManager board, string userId)
    {
        if (board == null)
        {
            Debug.LogWarning("[FS] Save 실패: BoardManager가 null입니다.");
            return;
        }

        try
        {
            MergeBoardData doc = new MergeBoardData();

            // BoardManager의 모든 셀 상태를 CellData로 변환
            foreach (Cells cell in board.GetAllCells())
            {
                CellData cd = new CellData
                {
                    x = cell.gridX,
                    y = cell.gridY,
                    isActive = cell.isActive,
                    donutID = cell.donutID
                };

                // 만약 donutID가 비어 있는데 occupant와 sprite가 있으면 ID 채우기
                if (string.IsNullOrEmpty(cd.donutID) && cell.occupant != null)
                {
                    Image img = cell.occupant.GetComponent<Image>();
                    if (img != null && img.sprite != null)
                    {
                        cd.donutID = DonutDatabase.GetIDBySprite(img.sprite);
                    }
                }

                doc.cells.Add(cd);
            }

            // 경로 통일: /users/{userId}/boards/board
            DocumentReference docRef = db
                .Collection("users")
                .Document(userId)
                .Collection("boards")
                .Document("board");

            await docRef.SetAsync(doc, SetOptions.Overwrite);

            Debug.Log($"[FS] Firestore 보드 저장 완료 → /users/{userId}/boards/board ({doc.cells.Count}칸)");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FS] 보드 저장 중 오류 발생: {e.Message}");
        }

        //// 보드 상태 수집
        //MergeBoardData data = new MergeBoardData();

        //foreach (Cells cell in boardData.GetAllCells())
        //{
        //    CellData cd = new CellData
        //    {
        //        x = cell.gridX,
        //        y = cell.gridY,
        //        isActive = cell.isActive,
        //        donutID = cell.donutID
        //    };

        //    if (cell.occupant != null)
        //    {
        //        Image img = cell.occupant.GetComponent<Image>();
        //        if (img != null && img.sprite != null)
        //        {
        //            cd.donutID = DonutDatabase.GetIDBySprite(img.sprite);
        //        }
        //    }

        //    data.cells.Add(cd);
        //}

        //// Firestore 문서 경로: user/{uid}/boards/{boardId}
        //var docRef = db.Collection("user")
        //               .Document(userId)
        //               .Collection("boards")
        //               .Document(boardId);

        //await docRef.SetAsync(data, SetOptions.Overwrite);

        //Debug.Log($"[FS] Firestore 보드 저장 완료 → user/{userId}/boards/{boardId} ({data.cells.Count}칸)");
    }

    // Firestore에서 불러오기
    public static async Task LoadToBoard(BoardManager board, string userId)
    {
        if (board == null)
        {
            Debug.LogWarning("[FS] Load 실패: BoardManager가 null입니다.");
            return;
        }

        try
        {
            DocumentReference docRef = db
                .Collection("users")
                .Document(userId)
                .Collection("boards")
                .Document("board");

            DocumentSnapshot snap = await docRef.GetSnapshotAsync();

            if (!snap.Exists)
            {
                Debug.Log($"[FS] 저장된 보드 데이터가 없습니다. /users/{userId}/boards/board");
                return;
            }

            MergeBoardData data = snap.ConvertTo<MergeBoardData>();
            if (data == null || data.cells == null)
            {
                Debug.LogWarning("[FS] 보드 데이터가 비어 있습니다.");
                return;
            }

            // Firestore 데이터 → 실제 보드에 반영
            foreach (CellData cd in data.cells)
            {
                Cells cell = board.GetCell(cd.x, cd.y);
                if (cell == null) continue;

                cell.SetActive(cd.isActive);
                cell.ClearItem();

                if (!string.IsNullOrEmpty(cd.donutID))
                {
                    Sprite sprite = DonutDatabase.GetSpriteByID(cd.donutID);
                    if (sprite == null)
                    {
                        Debug.LogWarning($"[FS] 스프라이트 '{cd.donutID}'를 찾을 수 없습니다.");
                        continue;
                    }

                    GameObject obj = Object.Instantiate(board.donutPrefab, cell.transform);
                    RectTransform rt = obj.GetComponent<RectTransform>();
                    rt.anchoredPosition = Vector2.zero;
                    rt.localScale = Vector3.one;

                    Image img = obj.GetComponent<Image>();
                    img.sprite = sprite;

                    MergeItemUI item = obj.GetComponent<MergeItemUI>();
                    item.donutID = cd.donutID;   // ID 동기화
                    cell.SetItem(item);
                }
            }

            Debug.Log($"[FS] Firestore 보드 불러오기 완료 ({data.cells.Count}칸)");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FS] 보드 불러오기 중 오류: {e.Message}");
        }
    }


    //public static async Task LoadFromFirestore(BoardManager board, string userId, string boardId = "stage1")
    //{
    //    if (board == null)
    //    {
    //        Debug.LogWarning("BoardSaveManager.LoadFromFirestore() : board가 null");
    //        return;
    //    }

    //    var docRef = db.Collection("user")
    //                   .Document(userId)
    //                   .Collection("boards")
    //                   .Document(boardId);

    //    var snap = await docRef.GetSnapshotAsync();

    //    if (!snap.Exists)
    //    {
    //        Debug.LogWarning($"[FS] Firestore에 보드 데이터가 없습니다: {boardId}");
    //        return;
    //    }

    //    MergeBoardData data = snap.ConvertTo<MergeBoardData>();

    //    // Firestore에서 받아온 데이터를 실제 보드에 적용
    //    foreach (CellData cd in data.cells)
    //    {
    //        Cells cell = board.GetCell(cd.x, cd.y);
    //        if (cell == null) continue;

    //        cell.SetActive(cd.isActive);
    //        cell.ClearItem();

    //        if (!string.IsNullOrEmpty(cd.donutID))
    //        {
    //            Sprite sprite = DonutDatabase.GetSpriteByID(cd.donutID);
    //            if (sprite == null)
    //            {
    //                Debug.LogWarning($"Sprite ID '{cd.donutID}'를 찾을 수 없습니다.");
    //                continue;
    //            }

    //            GameObject obj = Object.Instantiate(board.donutPrefab, cell.transform);
    //            RectTransform rt = obj.GetComponent<RectTransform>();
    //            rt.anchoredPosition = Vector2.zero;
    //            rt.localScale = Vector3.one;

    //            Image img = obj.GetComponent<Image>();
    //            img.sprite = sprite;

    //            MergeItemUI item = obj.GetComponent<MergeItemUI>();
    //            cell.SetItem(item);
    //        }
    //    }

    //    Debug.Log($"[FS] Firestore 보드 불러오기 완료 ({data.cells.Count}칸)");
    //}

    // Firestore에서 해당 보드 삭제
    public static async Task DeleteFromFirestore(string userId)
    {
        try
        {
            DocumentReference docRef = db
                .Collection("users")
                .Document(userId)
                .Collection("boards")
                .Document("board");

            await docRef.DeleteAsync();
            Debug.Log($"[FS] 보드 데이터 삭제 완료 → /users/{userId}/boards/board");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FS] 보드 삭제 중 오류: {e.Message}");
        }
    }
}
