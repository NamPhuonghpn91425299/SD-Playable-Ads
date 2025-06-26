Shader "Custom/CutoutShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" "RenderType" = "Opaque" }
        Pass
        {
            // Thiết lập Stencil
            Stencil
            {
                Ref 1            // Giá trị tham chiếu
                Comp Always      // Luôn ghi giá trị vào Stencil
                Pass Replace     // Ghi đè giá trị tham chiếu vào Stencil Buffer
            }

            ColorMask 0         // Không ghi màu (chỉ thao tác với stencil)
        }
    }
}
