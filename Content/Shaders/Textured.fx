﻿float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;
 
float4 AmbientColor = float4(1, 1, 1, 1);
float AmbientIntensity = 0.1;
 
float3 DiffuseLightDirection = float3(1, 0, 0);
float4 DiffuseColor = float4(1, 1, 1, 1);
float DiffuseIntensity = 1.0;
 
float Shininess = 200;
float4 SpecularColor = float4(1, 1, 1, 1);
float SpecularIntensity = 1;
float3 ViewVector = float3(1, 0, 0);
 
texture ModelTexture;
texture LightMap;
sampler2D textureSampler = sampler_state {
    Texture = (ModelTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};
sampler2D lightMapSampler = sampler_state {
    Texture = (LightMap);
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};
 
struct VertexShaderInput
{
    float4 Position : SV_POSITION;
    float4 Normal : NORMAL0;
    float2 TextureCoordinate : TEXCOORD0;
};
 
struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 Normal : TEXCOORD0;
    float2 TextureCoordinate : TEXCOORD1;
};
 
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
 
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
 
    float4 normal = normalize(mul(input.Normal, WorldInverseTranspose));
    float lightIntensity = dot(normal.xyz, DiffuseLightDirection);

    if(lightIntensity < 0.1f)
    	lightIntensity = 0.1f;

    output.Color = saturate(DiffuseColor * DiffuseIntensity * lightIntensity);
 
    output.Normal = normal.xyz;
 
    output.TextureCoordinate = input.TextureCoordinate;
    return output;
}
 
float4 PixelShaderFunction(VertexShaderOutput input) : SV_TARGET0
{
    float3 light = normalize(DiffuseLightDirection);
    float3 normal = normalize(input.Normal);
    float3 r = normalize(2 * dot(light, normal) * normal - light);
    float3 v = normalize(mul(float4(normalize(ViewVector),0), World)).xyz;
    float dotProduct = dot(r, v);
 
    float4 specular = SpecularIntensity * SpecularColor * max(pow(abs(dotProduct), Shininess), 0) * length(input.Color);
 
    float4 textureColor = tex2D(textureSampler, input.TextureCoordinate) * tex2D(lightMapSampler, input.TextureCoordinate);
    textureColor.a = 1;
 
    return saturate(textureColor * (input.Color) + AmbientColor * AmbientIntensity + specular);
}
 
technique Textured
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}