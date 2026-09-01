Shader "SphericalHarmonics/VertexColor"
{
    Properties { _Tint("Tint", Color) = (1,1,1,1) _Directional("Directional", Float) = 1 }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; fixed4 color:COLOR; };
            struct v2f { float4 vertex:SV_POSITION; fixed4 color:COLOR; float light:TEXCOORD0; };
            fixed4 _Tint; float _Directional;
            v2f vert(appdata v) { v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.color=v.color*_Tint; float directional=.3+.7*saturate(dot(UnityObjectToWorldNormal(v.normal),normalize(float3(.3,.6,-.7)))); o.light=lerp(1,directional,saturate(_Directional)); return o; }
            fixed4 frag(v2f i):SV_Target { return fixed4(i.color.rgb*i.light,i.color.a); }
            ENDCG
        }
    }
}
