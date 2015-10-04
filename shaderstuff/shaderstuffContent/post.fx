float4x4 WVP;
float Scale = 1;

sampler screenSamp : register(s0);

float4 ExtractPS(float2 input : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(screenSamp, input * Scale);
	color.a = color.r + color.b + color.g;
    return color;
}

technique BloomExtract
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 ExtractPS();
    }
}
float Kernel[] =
{
    -6,
    -5,
    -4,
    -3,
    -2,
    -1,
     0,
     1,
     2,
     3,
     4,
     5,
     6,
};

float Weights[] = {
   0.002216,
   0.008764,
   0.026995,
   0.064759,
   0.120985,
   0.176033,
   0.199471,
   0.176033,
   0.120985,
   0.064759,
   0.026995,
   0.008764,
   0.002216
};
float2 pixel;
float2 BlurAxis;
#define BLUR_SIZE 13
float4 BlurPS(float2 input : TEXCOORD0) : COLOR0
{
	float4 color = 0;
	for (int i = 0; i < BLUR_SIZE; i++) {
		color += tex2D(screenSamp, (input + (float2(Kernel[i], Kernel[i]) * BlurAxis * pixel)) * Scale) * Weights[i];
	}
    return color;
}
technique Blur
{
    pass blur
    {
        PixelShader = compile ps_2_0 BlurPS();
    }
}

float2 LightUV;
float Decay = .6;
#define SAMPLES 8
float4 CrepuscularPS(float2 input : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(screenSamp, input);
	float2 samp = 0;
	float decay = 1;
	float delta = 1.f / SAMPLES;
	for (int i = 0; i <= SAMPLES; i++){
		samp = lerp(input, LightUV, i*delta);
		color += tex2D(screenSamp, samp) * decay;
		decay *= Decay;
	}
    return color;
}
technique Crepuscular
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 CrepuscularPS();
    }
}
float4 NormalPS(float2 input : TEXCOORD0) : COLOR0
{
	return tex2D(screenSamp, input * Scale);
}
technique Normal
{
    pass pass1
    {
        PixelShader = compile ps_2_0 NormalPS();
    }
}