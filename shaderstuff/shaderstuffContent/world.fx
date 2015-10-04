float4x4 W;
float4x4 WIT;
float4x4 VP;

float3 LightPos = 0;
float4 LightColor = float4(1,1,1,1);

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
	float4 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float3 worldPos : TEXCOORD1;
	float4 Color : COLOR0;
	float4 normal : TEXCOORD2;
};

VertexShaderOutput RenderVS(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, W);
    output.Position = mul(worldPosition, VP);
	output.normal = mul(input.Normal,WIT);
	output.worldPos = worldPosition.xyz;
	output.Color = input.Color;

    return output;
}

float4 LambertPS(VertexShaderOutput input) : COLOR0
{
	float4 color = input.Color;
	float4 norm = normalize(input.normal);
	float3 light = normalize(input.worldPos-LightPos);
	float lambert = max(dot(norm, light), 0);
	color *= lambert * LightColor;

    return color;
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

float4 LightPS(VertexShaderOutput input) : COLOR0
{
	return LightColor;
}
technique Light
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 RenderVS();
        PixelShader = compile ps_2_0 LightPS();
    }
}