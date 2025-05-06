using UnityEngine;
using System.Collections.Generic;
using System.Text; // Để dùng StringBuilder cho log đẹp hơn

public class PoisonExperimentManager : MonoBehaviour
{
    public int numberOfBottles = 100;
    public int numberOfMice = 10; // Chúng ta biết cần 7, nhưng đề bài cho 10

    [SerializeField] private int poisonedBottleId = -1;
    [SerializeField] private List<Mouse> mice;

    // Class đơn giản để đại diện cho chuột
    [System.Serializable]
    private class Mouse
    {
        public int id; // ID của chuột, tương ứng vị trí bit (0 đến 9)
        public List<int> bottlesToDrinkFrom; // Danh sách các lọ chuột này cần uống
        public bool isDead;

        public Mouse(int mouseId)
        {
            id = mouseId;
            bottlesToDrinkFrom = new List<int>();
            isDead = false;
        }
    }

    void Start()
    {
        RunExperiment();
    }

    [ContextMenu("Run Experiment Again")] // Thêm nút để chạy lại từ Inspector
    void RunExperiment()
    {
        Debug.Log("=== BẮT ĐẦU THÍ NGHIỆM MỚI ===");

        // 1. Chuẩn bị lọ và chọn lọ độc ngẫu nhiên
        poisonedBottleId = Random.Range(1, numberOfBottles + 1); // ID từ 1 đến 100
        Debug.Log($"Đã chuẩn bị {numberOfBottles} lọ. Lọ độc là: #{poisonedBottleId}");

        // 2. Chuẩn bị chuột
        mice = new List<Mouse>();
        for (int i = 0; i < numberOfMice; i++)
        {
            mice.Add(new Mouse(i)); // Gán ID chuột từ 0 đến 9 (tương ứng bit 0-9)
        }
        Debug.Log($"Đã chuẩn bị {numberOfMice} con chuột.");

        // 3. Xác định chuột nào uống lọ nào (Dựa trên bit)
        AssignDrinks();

        // 4. Mô phỏng cho chuột uống và chờ kết quả (24h sau - ở đây là tức thì)
        SimulateOutcome();

        // 5. Giải mã kết quả từ trạng thái chuột
        DecodeResult();

        Debug.Log("=== KẾT THÚC THÍ NGHIỆM ===");
    }

    void AssignDrinks()
    {
        Debug.Log("--- Giai đoạn phân công uống thuốc ---");
        StringBuilder logBuilder = new StringBuilder();

        for (int bottleId = 1; bottleId <= numberOfBottles; bottleId++)
        {
            logBuilder.Clear();
            logBuilder.Append($"Lọ #{bottleId} (Binary: {System.Convert.ToString(bottleId, 2).PadLeft(numberOfMice, '0')}) -> Cho chuột: ");
            bool drankByAny = false;

            for (int mouseIndex = 0; mouseIndex < numberOfMice; mouseIndex++)
            {
                // Kiểm tra xem bit thứ 'mouseIndex' của 'bottleId' có được bật (là 1) hay không
                // Dùng phép dịch bit (>>) và phép AND bit (&)
                // (bottleId >> mouseIndex) dịch các bit của bottleId sang phải mouseIndex lần
                // & 1 kiểm tra xem bit cuối cùng (là bit ban đầu ở vị trí mouseIndex) có phải là 1 không
                if (((bottleId >> mouseIndex) & 1) == 1)
                {
                    mice[mouseIndex].bottlesToDrinkFrom.Add(bottleId);
                    logBuilder.Append($"#{mouseIndex} "); // Log ID chuột (0-9)
                    drankByAny = true;
                }
            }
            if (!drankByAny) logBuilder.Append("(Không có)");
           // Debug.Log(logBuilder.ToString()); // Bỏ comment nếu muốn xem chi tiết từng lọ
        }
         Debug.Log("Đã phân công xong. Mỗi chuột biết mình cần uống từ những lọ nào.");

        // Optional: In ra danh sách uống của từng chuột
        // foreach (Mouse mouse in mice)
        // {
        //     Debug.Log($"Chuột #{mouse.id} sẽ uống từ {mouse.bottlesToDrinkFrom.Count} lọ.");
        // }
    }

    void SimulateOutcome()
    {
        Debug.Log("--- Giai đoạn mô phỏng kết quả sau 24h ---");
        foreach (Mouse mouse in mice)
        {
            // Kiểm tra xem chuột này có uống phải lọ độc không
            if (mouse.bottlesToDrinkFrom.Contains(poisonedBottleId))
            {
                mouse.isDead = true;
                Debug.Log($"Chuột #{mouse.id} đã uống phải lọ độc #{poisonedBottleId} và CHẾT.");
            }
            else
            {
                mouse.isDead = false;
                // Debug.Log($"Chuột #{mouse.id} SỐNG SÓT."); // Bỏ comment nếu muốn xem chuột nào sống
            }
        }
    }

    void DecodeResult()
    {
        Debug.Log("--- Giai đoạn giải mã kết quả ---");
        int decodedId = 0;
        StringBuilder binaryResult = new StringBuilder(); // Để hiển thị chuỗi bit kết quả

        Debug.Log("Trạng thái chuột (0=Sống, 1=Chết):");
        // Lặp ngược từ chuột có ID cao nhất (bit cao nhất) xuống thấp nhất
        for (int i = numberOfMice - 1; i >= 0; i--)
        {
             Mouse mouse = mice[i]; // Lấy chuột tương ứng với bit vị trí i
             if (mouse.isDead)
             {
                 // Nếu chuột chết, bật bit tương ứng trong kết quả giải mã
                 // Dùng phép OR bit (|) và dịch bit trái (<<)
                 // (1 << mouse.id) tạo ra một số có bit thứ mouse.id là 1, các bit khác là 0
                 decodedId |= (1 << mouse.id); // Hoặc decodedId = decodedId | (1 << mouse.id);
                 binaryResult.Append("1"); // Thêm vào chuỗi hiển thị
                 Debug.Log($" - Chuột #{mouse.id}: CHẾT (Bit {mouse.id} = 1)");
             }
             else
             {
                 binaryResult.Append("0"); // Thêm vào chuỗi hiển thị
                  Debug.Log($" - Chuột #{mouse.id}: SỐNG (Bit {mouse.id} = 0)");
             }
        }

        Debug.Log($"Chuỗi bit kết quả (Từ bit {numberOfMice-1} đến 0): {binaryResult.ToString()}");
        Debug.Log($"=> Số giải mã được (thập phân): {decodedId}");

        // So sánh kết quả
        if (decodedId == poisonedBottleId)
        {
            Debug.Log($"<color=green>THÀNH CÔNG! Đã xác định đúng lọ độc là #{poisonedBottleId}.</color>");
        }
        else
        {
            Debug.Log($"<color=red>THẤT BẠI! Xác định nhầm lọ #{decodedId} trong khi lọ độc thực sự là #{poisonedBottleId}.</color>");
            Debug.LogError("Có lỗi trong thuật toán hoặc quá trình mô phỏng!");
        }
    }
}