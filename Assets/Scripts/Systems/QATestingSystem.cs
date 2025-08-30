using UnityEngine;

public static class QATestingSystem
{
    public static void RunAllTests()
    {
        TestGridCell();
    }

    public static void TestGridCell()
    {
        Debug.Log("[QATestingSystem] Starting tests for GridCell.");

        // Prueba de creación de celdas
        var cell = new GridCell(CellType.Empty);
        Debug.Assert(cell.Type == CellType.Empty, "[QATestingSystem] Error: Tipo de celda no coincide.");

        // Prueba de ocupantes
        cell.AddOccupant(CellOccupant.Robot);
        Debug.Assert(cell.HasOccupant(CellOccupant.Robot), "[QATestingSystem] Error: Robot no encontrado como ocupante.");

        UnityEngine.Debug.Log(cell.IsWalkableNow); // true

        cell.AddOccupant(CellOccupant.Robot);
        UnityEngine.Debug.Log(cell.IsWalkableNow); // false

        cell.RemoveOccupant(CellOccupant.Robot);
        cell.SetType(CellType.Wall);
        UnityEngine.Debug.Log(cell.IsWalkableNow); // false por tipo (muro)

        Debug.Log("[QATestingSystem] Todas las pruebas para GridCell completadas.");
    }
}
