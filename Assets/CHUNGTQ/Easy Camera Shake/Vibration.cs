using UnityEngine;
using System.Collections; // Cần thiết cho IEnumerator (Coroutines)

/// <summary>
/// Vibration (Camera Shake) Script
/// Author: MutantGopher (Nguyên bản, có thể đã được chỉnh sửa)
/// Description: Rung một GameObject. Thường dùng cho hiệu ứng rung camera,
/// nhưng có thể áp dụng cho bất kỳ GameObject nào.
/// Gắn script này vào GameObject bạn muốn rung và điều chỉnh các thiết lập.
/// Có thể gọi các hàm StartShaking(), StartShakingRandom(), và StopShaking()
/// từ các script khác cho các hiệu ứng như cháy nổ.
/// </summary>
public class Vibration : MonoBehaviour
{
    // --- Biến Public (Có thể chỉnh sửa trong Inspector) ---
    public bool vibrateOnAwake = true;                  // GameObject có tự động rung khi Awake không?
    public Vector3 startingShakeDistance;               // Khoảng cách tối đa mà GameObject sẽ di chuyển khi rung (theo mỗi trục)
    public Quaternion startingRotationAmount;           // Lượng xoay tối đa mà GameObject sẽ thực hiện khi rung (thành phần x,y,z của Quaternion)
                                                        // Lưu ý: Việc dùng trực tiếp các thành phần x,y,z của Quaternion để xoay có thể không trực quan.
                                                        // Thường người ta sẽ dùng Euler angles hoặc một Vector3 đại diện cho góc xoay.
    public float shakeSpeed = 60.0f;                    // Tốc độ của chuyển động rung (ảnh hưởng đến tần suất của hàm Sin)
    public float decreaseMultiplier = 0.5f;             // Hệ số giảm biên độ rung sau mỗi chu kỳ (0 đến 1).
                                                        // 0.5 nghĩa là sau mỗi chu kỳ, biên độ rung giảm đi một nửa.
    public int numberOfShakes = 8;                      // Số chu kỳ rung trước khi dừng (nếu shakeContinuous là false)
    public bool shakeContinuous = false;                // Rung liên tục hay chỉ một lần?

    // --- Biến Private (Dùng nội bộ trong script) ---
    private Vector3 actualStartingShakeDistance;        // Khoảng cách rung thực tế được sử dụng (có thể thay đổi bởi các hàm StartShaking khác nhau)
    private Quaternion actualStartingRotationAmount;    // Lượng xoay thực tế được sử dụng
    private float actualShakeSpeed;                     // Tốc độ rung thực tế
    private float actualDecreaseMultiplier;             // Hệ số giảm thực tế
    private int actualNumberOfShakes;                   // Số chu kỳ rung thực tế

    private Vector3 originalPosition;                   // Vị trí ban đầu của GameObject trước khi rung
    private Quaternion originalRotation;                // Xoay ban đầu của GameObject trước khi rung
    public static Vibration Instance { get; private set; } // Biến tĩnh để truy cập từ các script khác
    void Awake()
    {
        Instance = this; // Gán Instance để có thể truy cập từ các script khác
        
        // Lưu lại vị trí và xoay ban đầu của GameObject khi script được khởi tạo
        originalPosition = transform.localPosition; // Sử dụng localPosition để rung tương đối với parent
        originalRotation = transform.localRotation; // Sử dụng localRotation

        if (vibrateOnAwake)
        {
            StartShaking(); // Bắt đầu rung ngay nếu được thiết lập
        }
    }

#if UNITY_EDITOR // Đoạn code này chỉ được biên dịch và chạy trong Unity Editor
    void Update()
    {
        // Cho phép test nhanh hiệu ứng rung bằng cách nhấn phím L trong Editor
        if (Input.GetKeyDown(KeyCode.L))
        {
            // // Tính toán một "sức mạnh" rung dựa trên CameraShakeViolence
            // // CameraShakeViolence càng lớn, shakeViolence càng nhỏ (rung nhẹ hơn)
            // // CameraShakeViolence càng nhỏ (gần 0), shakeViolence càng lớn (rung mạnh hơn)
            // float shakeViolence = 1 / (5 * CameraShakeViolence); // Cẩn thận chia cho 0 nếu CameraShakeViolence = 0
            // if (CameraShakeViolence == 0) shakeViolence = 0.2f; // Xử lý trường hợp chia cho 0
            //
            // // Bắt đầu rung với các giá trị ngẫu nhiên dựa trên shakeViolence
            // // Các tham số là minDistance, maxDistance, minRotation, maxRotation
            // // Ở đây, minDistance = -shakeViolence, maxDistance = shakeViolence,
            // // nghĩa là nó có thể rung theo cả hướng dương và âm.
            // StartShakingRandom(-shakeViolence, shakeViolence, -shakeViolence, shakeViolence);
            StartShaking();
        }
    }
#endif

    // --- Các Hàm Public để Kích Hoạt Rung ---

    // Bắt đầu rung với các giá trị mặc định đã thiết lập trong Inspector
    public void StartShaking()
    {
        // Gán các giá trị mặc định (từ Inspector) cho các biến "actual"
        actualStartingShakeDistance = startingShakeDistance;
        actualStartingRotationAmount = startingRotationAmount;
        actualShakeSpeed = shakeSpeed;
        actualDecreaseMultiplier = decreaseMultiplier;
        actualNumberOfShakes = numberOfShakes;

        StopShaking(); // Dừng coroutine "Shake" hiện tại (nếu có) và reset vị trí/xoay
        StartCoroutine("Shake"); // Bắt đầu một coroutine "Shake" mới
    }

    // Bắt đầu rung với các giá trị tùy chỉnh được truyền vào
    public void StartShaking(Vector3 shakeDistance, Quaternion rotationAmount, float speed, float diminish, int numOfShakes)
    {
        actualStartingShakeDistance = shakeDistance;
        actualStartingRotationAmount = rotationAmount;
        actualShakeSpeed = speed;
        actualDecreaseMultiplier = diminish;
        actualNumberOfShakes = numOfShakes;

        StopShaking();
        StartCoroutine("Shake");
    }

    // Bắt đầu rung với một khoảng cách rung tổng thể và một góc xoay tổng thể
    // Các giá trị cụ thể cho mỗi trục sẽ được random hóa
    public void StartShaking(float totalShakeDistance, float totalShakeAngle)
    {
        var halfMaxShakeDistance = totalShakeDistance / 2;
        // Tạo khoảng cách rung ngẫu nhiên cho mỗi trục, trong khoảng [-halfMax, +halfMax]
        actualStartingShakeDistance = new Vector3(
            Random.Range(-halfMaxShakeDistance, halfMaxShakeDistance),
            Random.Range(-halfMaxShakeDistance, halfMaxShakeDistance),
            Random.Range(-halfMaxShakeDistance, halfMaxShakeDistance)
        );

        var halfMaxShakeAngle = totalShakeAngle / 2;
        // Tạo lượng xoay ngẫu nhiên cho mỗi thành phần của Quaternion
        // Lưu ý: Việc random trực tiếp các thành phần x,y,z của Quaternion và giữ w=1
        // có thể không tạo ra các phép xoay trực quan hoặc mong muốn.
        // Quaternion hợp lệ nên được chuẩn hóa (normalized).
        // Cách tốt hơn là random Euler angles rồi chuyển sang Quaternion.
        actualStartingRotationAmount = new Quaternion(
            Random.Range(-halfMaxShakeAngle, halfMaxShakeAngle),
            Random.Range(-halfMaxShakeAngle, halfMaxShakeAngle),
            Random.Range(-halfMaxShakeAngle, halfMaxShakeAngle),
            1 // Giữ w = 1 không phải lúc nào cũng tạo Quaternion hợp lệ cho xoay lớn.
              // Quaternion nên được normalize sau khi random các thành phần.
        ).normalized; // THÊM .normalized ĐỂ CÓ QUATERNION HỢP LỆ HƠN

        // Random hóa một chút tốc độ, hệ số giảm, và số lần rung
        actualShakeSpeed = shakeSpeed * Random.Range(0.8f, 1.2f);
        actualDecreaseMultiplier = decreaseMultiplier * Random.Range(0.8f, 1.2f);
        actualNumberOfShakes = Mathf.Max(1, numberOfShakes + Random.Range(-2, 2)); // Đảm bảo ít nhất 1 lần rung

        StopShaking();
        StartCoroutine("Shake");
    }

    // Bắt đầu rung với khoảng cách và lượng xoay ngẫu nhiên trong một khoảng min/max
    public void StartShakingRandom(float minDistance, float maxDistance, float minRotationAmount, float maxRotationAmount)
    {
        actualStartingShakeDistance = new Vector3(
            Random.Range(minDistance, maxDistance),
            Random.Range(minDistance, maxDistance),
            Random.Range(minDistance, maxDistance)
        );
        actualStartingRotationAmount = new Quaternion(
            Random.Range(minRotationAmount, maxRotationAmount),
            Random.Range(minRotationAmount, maxRotationAmount),
            Random.Range(minRotationAmount, maxRotationAmount),
            1
        ).normalized; // THÊM .normalized

        actualShakeSpeed = shakeSpeed * Random.Range(0.8f, 1.2f);
        actualDecreaseMultiplier = decreaseMultiplier * Random.Range(0.8f, 1.2f);
        actualNumberOfShakes = Mathf.Max(1, numberOfShakes + Random.Range(-2, 2));

        StopShaking();
        StartCoroutine("Shake");
    }

    // Dừng hiệu ứng rung ngay lập tức và reset vị trí/xoay
    public void StopShaking()
    {
        StopCoroutine("Shake"); // Dừng coroutine "Shake" nếu nó đang chạy

        // Reset vị trí và xoay về trạng thái ban đầu đã lưu trong Awake()
        // Quan trọng: Nếu originalPosition/Rotation chưa được cập nhật đúng trong coroutine Shake()
        // trước khi một StartShaking() mới được gọi, điều này có thể gây ra vấn đề.
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
    }

    // --- Coroutine Thực Hiện Rung ---
    private IEnumerator Shake()
    {
        // Lưu lại vị trí và xoay hiện tại của GameObject TRƯỚC KHI bắt đầu chu kỳ rung này.
        // Điều này quan trọng để mỗi chu kỳ rung dựa trên vị trí "tĩnh" ban đầu,
        // không phải vị trí bị lệch của lần rung trước.
        // Tuy nhiên, originalPosition/Rotation đã được lưu trong Awake().
        // Việc gán lại ở đây có thể cần thiết nếu trạng thái "nghỉ" có thể thay đổi giữa các lần gọi Shake.
        // Nhưng nếu mục tiêu là luôn quay về vị trí lúc Awake, thì không cần gán lại ở đây.
        // Để an toàn và theo logic hiện tại (reset về vị trí trước khi shake), nên giữ lại.
        Vector3 localOriginalPosition = transform.localPosition;
        Quaternion localOriginalRotation = transform.localRotation;

        float hitTime = Time.time;    // Thời điểm bắt đầu của chu kỳ rung hiện tại (dùng để tính toán cho hàm Sin)
        float shake = actualNumberOfShakes; // Số chu kỳ rung còn lại

        // Lưu trữ biên độ rung ban đầu cho vị trí và xoay
        float shakeDistanceX = actualStartingShakeDistance.x;
        float shakeDistanceY = actualStartingShakeDistance.y;
        float shakeDistanceZ = actualStartingShakeDistance.z;

        float shakeRotationX = actualStartingRotationAmount.x;
        float shakeRotationY = actualStartingRotationAmount.y;
        float shakeRotationZ = actualStartingRotationAmount.z;

        // Vòng lặp chính thực hiện rung
        // Tiếp tục lặp nếu số chu kỳ rung (shake) còn lại > 0 HOẶC nếu shakeContinuous là true
        while (shake > 0 || shakeContinuous)
        {
            // Tính toán bộ đếm thời gian cho hàm Sin, dựa trên tốc độ rung
            float timer = (Time.time - hitTime) * actualShakeSpeed;

            // Tính toán vị trí mới dựa trên hàm Sin (tạo dao động) và biên độ rung hiện tại
            // GameObject sẽ di chuyển qua lại quanh localOriginalPosition
            float x = localOriginalPosition.x + Mathf.Sin(timer) * shakeDistanceX;
            float y = localOriginalPosition.y + Mathf.Sin(timer) * shakeDistanceY;
            float z = localOriginalPosition.z + Mathf.Sin(timer) * shakeDistanceZ;

            // Tính toán các thành phần xoay mới
            // LƯU Ý QUAN TRỌNG: Cộng trực tiếp vào các thành phần x,y,z của Quaternion
            // và giữ w=1 như thế này KHÔNG PHẢI là cách đúng để tạo hiệu ứng xoay.
            // Điều này sẽ làm biến dạng (skew/shear) GameObject thay vì xoay nó.
            // Để xoay, bạn nên:
            // 1. Tính toán góc xoay Euler (ví dụ: Vector3(angleX, angleY, angleZ)).
            // 2. Chuyển góc Euler đó thành Quaternion: Quaternion.Euler(angleX, angleY, angleZ).
            // 3. Nhân Quaternion gốc với Quaternion xoay này: localOriginalRotation * rotationOffset.
            float xr = localOriginalRotation.x + Mathf.Sin(timer) * shakeRotationX; // SAI VỀ MẶT TOÁN HỌC QUATERNION
            float yr = localOriginalRotation.y + Mathf.Sin(timer) * shakeRotationY; // SAI
            float zr = localOriginalRotation.z + Mathf.Sin(timer) * shakeRotationZ; // SAI

            // Áp dụng vị trí mới
            transform.localPosition = new Vector3(x, y, z);
            // Áp dụng xoay mới (SẼ GÂY BIẾN DẠNG)
            transform.localRotation = new Quaternion(xr, yr, zr, localOriginalRotation.w); // Nên giữ w gốc hoặc normalize
                                                                                          // Tốt nhất là tính toán xoay đúng cách (xem bình luận trên)

            // Kiểm tra xem một chu kỳ của hàm Sin (2 * PI) đã hoàn thành chưa
            if (timer > Mathf.PI * 2)
            {
                hitTime = Time.time; // Reset thời điểm bắt đầu cho chu kỳ tiếp theo

                // Giảm biên độ rung cho vị trí và xoay theo hệ số decreaseMultiplier
                shakeDistanceX *= actualDecreaseMultiplier;
                shakeDistanceY *= actualDecreaseMultiplier;
                shakeDistanceZ *= actualDecreaseMultiplier;

                shakeRotationX *= actualDecreaseMultiplier;
                shakeRotationY *= actualDecreaseMultiplier;
                shakeRotationZ *= actualDecreaseMultiplier;

                // Giảm số chu kỳ rung còn lại (nếu không phải là rung liên tục)
                if (!shakeContinuous) {
                    shake--;
                }
            }
            // Chờ đến frame tiếp theo trước khi lặp lại
            // `yield return true;` hoặc `yield return null;` đều có nghĩa là chờ 1 frame.
            yield return null;
        }

        // Sau khi vòng lặp kết thúc (hết số lần rung và không phải rung liên tục)
        // Reset vị trí và xoay của GameObject về trạng thái ban đầu đã lưu trong Awake()
        // (hoặc trạng thái lưu ở đầu Coroutine này nếu logic là vậy)
        transform.localPosition = originalPosition; // Nên là localOriginalPosition nếu muốn quay về vị trí trước khi coroutine này bắt đầu
        transform.localRotation = originalRotation; // Tương tự
    }
}