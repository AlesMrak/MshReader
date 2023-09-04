
// 3ds shader, handles submission of basic material
// group buffers
struct VS_OUTPUT
{
    float4 Pos    : POSITION;
    float3 Light  : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float3 View   : TEXCOORD2;
};



//
// Uniform effect parameters
//
float4x4 WorldViewProj     : WORLDVIEWPROJECTION;
float4x4 World;
float4x4 InvTransposeWorld;
float3   LightDir;
float3	 EyePosition;
float4   AddColor;
float4	 AmbientColor;
float4   DiffuseColor;
float4   SpecularColor;


//
// Vertex shader
//
VS_OUTPUT vs_main(
    float4 Pos  : POSITION,
    float3 Normal : NORMAL)
{
    // Declare our return variable
    VS_OUTPUT Out = (VS_OUTPUT)0;

    // Get transformed world position of vertex
    float4 posWorld = mul(Pos, World);

    // Transform our position
    Out.Pos = mul(Pos, WorldViewProj);
    Out.Normal = mul(Normal, InvTransposeWorld);
    Out.Light = LightDir;
    Out.View = EyePosition - posWorld;

    // Return
    return Out;
}

float4 ps_main(float3 Light : TEXCOORD0, float3 Norm : TEXCOORD1, float3 View : TEXCOORD2) : COLOR
{
    float3 N = normalize(Norm);
    float3 L = normalize(Light);
    float3 V = normalize(View);

	float dI = saturate(dot(N, L));

    // R = 2 * (N.L) * N - L
    float3 reflect = normalize(2 * dI * N - L);

    // R.V^n
    float sI = pow(saturate(dot(reflect, V)), 8);

	return (DiffuseColor * dI) + SpecularColor * sI; // (AmbientColor * I);// + (SpecularColor); // float4(abs(Norm), 1);
}

technique Simplest
{
    pass Pass0
    {        
        VertexShader = compile vs_1_1 vs_main();
        PixelShader = compile ps_2_0 ps_main();
    }
}
