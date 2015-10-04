float4x4 W;
float4x4 WIT;
float4x4 VP;

float3 LightPos = 0;
float3 CameraPos = 0;
float4 LightColor = float4(1,1,1,1);

Texture tex;
sampler samp = sampler_state{
	Texture = <tex>;
	AddressU = Wrap;
	AddressV = Wrap;
};
Texture texlow;
sampler samplow = sampler_state{
	Texture = <texlow>;
	AddressU = Wrap;
	AddressV = Wrap;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float4 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float Light : TEXCOORD1;
	float Depth : TEXCOORD2;
};

VertexShaderOutput RenderVS(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, W);
    output.Position = mul(worldPosition, VP);
	output.UV = input.TexCoord;
	output.Light = dot(input.Normal, normalize(LightPos - worldPosition));
	output.Depth = length(worldPosition - CameraPos);

    return output;
}

float4 LambertPS(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(samp, input.UV);
	float4 collow = tex2D(samplow, input.UV);
	float fade = clamp(input.Depth/10, 0, 1);
	color *= 1-fade;
	collow *= fade;
	color += collow;
    return color * input.Light;
}

technique Lambert
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 RenderVS();
        PixelShader = compile ps_2_0 LambertPS();
    }
}

float4 OcculdPS(VertexShaderOutput input) : COLOR0
{
	return float4(0,0,0,1);
}
technique Occuld
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 RenderVS();
        PixelShader = compile ps_2_0 OcculdPS();
    }
}

struct LightVSin
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
};
struct LightVSout
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
};
VertexShaderOutput LightVS(LightVSin input)
{
    LightVSout output;

    output.Position = mul(mul(input.Position, W), VP);
	output.Color = input.Color;

    return output;
}
float4 LightPS(LightVSout input) : COLOR0
{
	return input.Color;
}
technique Light
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 LightVS();
        PixelShader = compile ps_2_0 LightPS();
    }
}