using UnityEngine;

public class ContainerTester : MonoBehaviour
{
    [Header("Map keys (1,2,3...) to containers")]
    public Container[] containers;


    [Header("Test Settings")]
    public float progressPerPress = 0.25f;

    private void Update()
    {
        // Manually check number keys
        if (Input.GetKeyDown(KeyCode.Alpha1)) HandleKey(0, "1");
        if (Input.GetKeyDown(KeyCode.Alpha2)) HandleKey(1, "2");
        if (Input.GetKeyDown(KeyCode.Alpha3)) HandleKey(2, "3");
        if (Input.GetKeyDown(KeyCode.Alpha4)) HandleKey(3, "4");
        if (Input.GetKeyDown(KeyCode.Alpha5)) HandleKey(4, "5");
        if (Input.GetKeyDown(KeyCode.Alpha6)) HandleKey(5, "6");
        if (Input.GetKeyDown(KeyCode.Alpha7)) HandleKey(6, "7");
        if (Input.GetKeyDown(KeyCode.Alpha8)) HandleKey(7, "8");
        if (Input.GetKeyDown(KeyCode.Alpha9)) HandleKey(8, "9");
        // add more if you really need them
    }

    private void HandleKey(int index, string keyLabel)
    {
        if (index < 0 || index >= containers.Length)
        {
            Debug.LogWarning($"[TEST] Key {keyLabel} pressed but no container at index {index}.");
            return;
        }

        var c = containers[index];
        if (c == null)
        {
            Debug.LogWarning($"[TEST] Key {keyLabel} pressed but container {index} is null.");
            return;
        }

        Debug.Log($"[TEST] Key {keyLabel} pressed. Filling Container {index + 1}.");
        c.AddProgress(progressPerPress);
        Debug.Log($"[TEST] Container {index + 1} → Progress = {c.Progress01:F2}, Filled = {c.IsFilled}");
    }

    private void FillContainer(int index)
    {
        if (index < 0 || index >= containers.Length)
        {
            Debug.LogWarning($"Invalid container index {index}");
            return;
        }

        var c = containers[index];
        if (c == null)
        {
            Debug.LogWarning($"Container {index} is null.");
            return;
        }

        c.AddProgress(progressPerPress);

        Debug.Log(
            $"[TEST] Filled Container {index + 1}: " +
            $"Progress = {c.Progress01:F2}, Filled = {c.IsFilled}"
        );
    }
}
