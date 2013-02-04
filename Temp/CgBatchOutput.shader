Shader "FX/Water (simple)" {
Properties {
	_horizonColor ("Horizon color", COLOR)  = ( .172 , .463 , .435 , 0)
	_WaveScale ("Wave scale", Range (0.02,0.15)) = .07
	_ColorControl ("Reflective color (RGB) fresnel (A) ", 2D) = "" { }
	_ColorControlCube ("Reflective color cube (RGB) fresnel (A) ", Cube) = "" { TexGen CubeReflect }
	_BumpMap ("Waves Normalmap ", 2D) = "" { }
	WaveSpeed ("Wave speed (map1 x,y; map2 x,y)", Vector) = (19,9,-16,-7)
	_MainTex ("Fallback texture", 2D) = "" { }
}

#LINE 54

	
// -----------------------------------------------------------
// Fragment program

Subshader {
	Tags { "RenderType"="Opaque" }
	Pass {

Program "vp" {
// Vertex combos: 1
//   opengl - ALU: 18 to 18
//   d3d9 - ALU: 18 to 18
//   d3d11 - ALU: 10 to 10, TEX: 0 to 0, FLOW: 1 to 1
//   d3d11_9x - ALU: 10 to 10, TEX: 0 to 0, FLOW: 1 to 1
SubProgram "opengl " {
Keywords { }
Bind "vertex" Vertex
Vector 9 [_WorldSpaceCameraPos]
Matrix 5 [_World2Object]
Vector 10 [unity_Scale]
Float 11 [_WaveScale]
Vector 12 [_WaveOffset]
"!!ARBvp1.0
# 18 ALU
PARAM c[13] = { { 0.40000001, 0.44999999, 1 },
		state.matrix.mvp,
		program.local[5..12] };
TEMP R0;
TEMP R1;
MOV R1.w, c[0].z;
MOV R1.xyz, c[9];
DP4 R0.z, R1, c[7];
DP4 R0.x, R1, c[5];
DP4 R0.y, R1, c[6];
MAD R1.xyz, R0, c[10].w, -vertex.position;
DP3 R0.x, R1, R1;
RSQ R1.w, R0.x;
RCP R0.z, c[10].w;
MUL R0.xy, vertex.position.xzzw, c[11].x;
MAD R0, R0.xyxy, R0.z, c[12];
MUL result.texcoord[2].xyz, R1.w, R1.xzyw;
MUL result.texcoord[0].xy, R0, c[0];
MOV result.texcoord[1].xy, R0.wzzw;
DP4 result.position.w, vertex.position, c[4];
DP4 result.position.z, vertex.position, c[3];
DP4 result.position.y, vertex.position, c[2];
DP4 result.position.x, vertex.position, c[1];
END
# 18 instructions, 2 R-regs
"
}

SubProgram "d3d9 " {
Keywords { }
Bind "vertex" Vertex
Matrix 0 [glstate_matrix_mvp]
Vector 8 [_WorldSpaceCameraPos]
Matrix 4 [_World2Object]
Vector 9 [unity_Scale]
Float 10 [_WaveScale]
Vector 11 [_WaveOffset]
"vs_2_0
; 18 ALU
def c12, 0.40000001, 0.44999999, 1.00000000, 0
dcl_position0 v0
mov r1.w, c12.z
mov r1.xyz, c8
dp4 r0.z, r1, c6
dp4 r0.x, r1, c4
dp4 r0.y, r1, c5
mad r1.xyz, r0, c9.w, -v0
dp3 r0.x, r1, r1
rsq r1.w, r0.x
rcp r0.z, c9.w
mul r0.xy, v0.xzzw, c10.x
mad r0, r0.xyxy, r0.z, c11
mul oT2.xyz, r1.w, r1.xzyw
mul oT0.xy, r0, c12
mov oT1.xy, r0.wzzw
dp4 oPos.w, v0, c3
dp4 oPos.z, v0, c2
dp4 oPos.y, v0, c1
dp4 oPos.x, v0, c0
"
}

SubProgram "d3d11 " {
Keywords { }
Bind "vertex" Vertex
ConstBuffer "$Globals" 80 // 80 used size, 5 vars
Float 48 [_WaveScale]
Vector 64 [_WaveOffset] 4
ConstBuffer "UnityPerCamera" 128 // 76 used size, 8 vars
Vector 64 [_WorldSpaceCameraPos] 3
ConstBuffer "UnityPerDraw" 336 // 336 used size, 6 vars
Matrix 0 [glstate_matrix_mvp] 4
Matrix 256 [_World2Object] 4
Vector 320 [unity_Scale] 4
BindCB "$Globals" 0
BindCB "UnityPerCamera" 1
BindCB "UnityPerDraw" 2
// 18 instructions, 1 temp regs, 0 temp arrays:
// ALU 10 float, 0 int, 0 uint
// TEX 0 (0 load, 0 comp, 0 bias, 0 grad)
// FLOW 1 static, 0 dynamic
"vs_4_0
eefiecedjkmljcdgdegfeceblloobmhmhepaloopabaaaaaaniadaaaaadaaaaaa
cmaaaaaahmaaaaaaaeabaaaaejfdeheoeiaaaaaaacaaaaaaaiaaaaaadiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaaebaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahaaaaaafaepfdejfeejepeoaaeoepfcenebemaaepfdeheo
iaaaaaaaaeaaaaaaaiaaaaaagiaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaa
apaaaaaaheaaaaaaaaaaaaaaaaaaaaaaadaaaaaaabaaaaaaadamaaaaheaaaaaa
abaaaaaaaaaaaaaaadaaaaaaacaaaaaaadamaaaaheaaaaaaacaaaaaaaaaaaaaa
adaaaaaaadaaaaaaahaiaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklfdeieefcmmacaaaaeaaaabaaldaaaaaafjaaaaaeegiocaaaaaaaaaaa
afaaaaaafjaaaaaeegiocaaaabaaaaaaafaaaaaafjaaaaaeegiocaaaacaaaaaa
bfaaaaaafpaaaaadpcbabaaaaaaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaa
gfaaaaaddccabaaaabaaaaaagfaaaaaddccabaaaacaaaaaagfaaaaadhccabaaa
adaaaaaagiaaaaacabaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaa
egiocaaaacaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaa
aaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaa
egiocaaaacaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaak
pccabaaaaaaaaaaaegiocaaaacaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaa
aaaaaaaadiaaaaaipcaabaaaaaaaaaaaigbibaaaaaaaaaaaagiacaaaaaaaaaaa
adaaaaaaaoaaaaaipcaabaaaaaaaaaaaegaobaaaaaaaaaaapgipcaaaacaaaaaa
beaaaaaaaaaaaaaipcaabaaaaaaaaaaaegaobaaaaaaaaaaaegiocaaaaaaaaaaa
aeaaaaaadiaaaaakdccabaaaabaaaaaaegaabaaaaaaaaaaaaceaaaaamnmmmmdo
ggggogdoaaaaaaaaaaaaaaaadgaaaaafdccabaaaacaaaaaalgapbaaaaaaaaaaa
diaaaaajhcaabaaaaaaaaaaafgifcaaaabaaaaaaaeaaaaaaigibcaaaacaaaaaa
bbaaaaaadcaaaaalhcaabaaaaaaaaaaaigibcaaaacaaaaaabaaaaaaaagiacaaa
abaaaaaaaeaaaaaaegacbaaaaaaaaaaadcaaaaalhcaabaaaaaaaaaaaigibcaaa
acaaaaaabcaaaaaakgikcaaaabaaaaaaaeaaaaaaegacbaaaaaaaaaaaaaaaaaai
hcaabaaaaaaaaaaaegacbaaaaaaaaaaaigibcaaaacaaaaaabdaaaaaadcaaaaal
hcaabaaaaaaaaaaaegacbaaaaaaaaaaapgipcaaaacaaaaaabeaaaaaaigbbbaia
ebaaaaaaaaaaaaaabaaaaaahicaabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaa
aaaaaaaaeeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaadiaaaaahhccabaaa
adaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaadoaaaaab"
}

SubProgram "gles " {
Keywords { }
"!!GLES
#define SHADER_API_GLES 1
#define tex2D texture2D


#ifdef VERTEX
#define gl_ModelViewProjectionMatrix glstate_matrix_mvp
uniform mat4 glstate_matrix_mvp;

varying highp vec3 xlv_TEXCOORD2;
varying highp vec2 xlv_TEXCOORD0_1;
varying highp vec2 xlv_TEXCOORD0;
uniform highp vec4 unity_Scale;

uniform highp vec3 _WorldSpaceCameraPos;
uniform highp mat4 _World2Object;
uniform highp float _WaveScale;
uniform highp vec4 _WaveOffset;
attribute vec4 _glesVertex;
void main ()
{
  highp vec4 temp_1;
  temp_1 = (((_glesVertex.xzxz * _WaveScale) / unity_Scale.w) + _WaveOffset);
  highp vec4 tmpvar_2;
  tmpvar_2.w = 1.00000;
  tmpvar_2.xyz = _WorldSpaceCameraPos;
  gl_Position = (gl_ModelViewProjectionMatrix * _glesVertex);
  xlv_TEXCOORD0 = (temp_1.xy * vec2(0.400000, 0.450000));
  xlv_TEXCOORD0_1 = temp_1.wz;
  xlv_TEXCOORD2 = normalize((((_World2Object * tmpvar_2).xyz * unity_Scale.w) - _glesVertex.xyz)).xzy;
}



#endif
#ifdef FRAGMENT

varying highp vec3 xlv_TEXCOORD2;
varying highp vec2 xlv_TEXCOORD0_1;
varying highp vec2 xlv_TEXCOORD0;
uniform highp vec4 _horizonColor;
uniform sampler2D _ColorControl;
uniform sampler2D _BumpMap;
void main ()
{
  mediump vec4 col_1;
  mediump vec4 water_2;
  mediump float fresnel_3;
  mediump vec3 bump2_4;
  mediump vec3 bump1_5;
  lowp vec3 tmpvar_6;
  tmpvar_6 = ((texture2D (_BumpMap, xlv_TEXCOORD0).xyz * 2.00000) - 1.00000);
  bump1_5 = tmpvar_6;
  lowp vec3 tmpvar_7;
  tmpvar_7 = ((texture2D (_BumpMap, xlv_TEXCOORD0_1).xyz * 2.00000) - 1.00000);
  bump2_4 = tmpvar_7;
  mediump vec3 tmpvar_8;
  tmpvar_8 = ((bump1_5 + bump2_4) * 0.500000);
  highp float tmpvar_9;
  tmpvar_9 = dot (xlv_TEXCOORD2, tmpvar_8);
  fresnel_3 = tmpvar_9;
  lowp vec4 tmpvar_10;
  tmpvar_10 = texture2D (_ColorControl, vec2(fresnel_3));
  water_2 = tmpvar_10;
  highp vec3 tmpvar_11;
  tmpvar_11 = mix (water_2.xyz, _horizonColor.xyz, water_2.www);
  col_1.xyz = tmpvar_11;
  highp float tmpvar_12;
  tmpvar_12 = _horizonColor.w;
  col_1.w = tmpvar_12;
  gl_FragData[0] = col_1;
}



#endif"
}

SubProgram "glesdesktop " {
Keywords { }
"!!GLES
#define SHADER_API_GLES 1
#define tex2D texture2D


#ifdef VERTEX
#define gl_ModelViewProjectionMatrix glstate_matrix_mvp
uniform mat4 glstate_matrix_mvp;

varying highp vec3 xlv_TEXCOORD2;
varying highp vec2 xlv_TEXCOORD0_1;
varying highp vec2 xlv_TEXCOORD0;
uniform highp vec4 unity_Scale;

uniform highp vec3 _WorldSpaceCameraPos;
uniform highp mat4 _World2Object;
uniform highp float _WaveScale;
uniform highp vec4 _WaveOffset;
attribute vec4 _glesVertex;
void main ()
{
  highp vec4 temp_1;
  temp_1 = (((_glesVertex.xzxz * _WaveScale) / unity_Scale.w) + _WaveOffset);
  highp vec4 tmpvar_2;
  tmpvar_2.w = 1.00000;
  tmpvar_2.xyz = _WorldSpaceCameraPos;
  gl_Position = (gl_ModelViewProjectionMatrix * _glesVertex);
  xlv_TEXCOORD0 = (temp_1.xy * vec2(0.400000, 0.450000));
  xlv_TEXCOORD0_1 = temp_1.wz;
  xlv_TEXCOORD2 = normalize((((_World2Object * tmpvar_2).xyz * unity_Scale.w) - _glesVertex.xyz)).xzy;
}



#endif
#ifdef FRAGMENT

varying highp vec3 xlv_TEXCOORD2;
varying highp vec2 xlv_TEXCOORD0_1;
varying highp vec2 xlv_TEXCOORD0;
uniform highp vec4 _horizonColor;
uniform sampler2D _ColorControl;
uniform sampler2D _BumpMap;
void main ()
{
  mediump vec4 col_1;
  mediump vec4 water_2;
  mediump float fresnel_3;
  mediump vec3 bump2_4;
  mediump vec3 bump1_5;
  lowp vec3 normal_6;
  normal_6.xy = ((texture2D (_BumpMap, xlv_TEXCOORD0).wy * 2.00000) - 1.00000);
  normal_6.z = sqrt(((1.00000 - (normal_6.x * normal_6.x)) - (normal_6.y * normal_6.y)));
  bump1_5 = normal_6;
  lowp vec3 normal_7;
  normal_7.xy = ((texture2D (_BumpMap, xlv_TEXCOORD0_1).wy * 2.00000) - 1.00000);
  normal_7.z = sqrt(((1.00000 - (normal_7.x * normal_7.x)) - (normal_7.y * normal_7.y)));
  bump2_4 = normal_7;
  mediump vec3 tmpvar_8;
  tmpvar_8 = ((bump1_5 + bump2_4) * 0.500000);
  highp float tmpvar_9;
  tmpvar_9 = dot (xlv_TEXCOORD2, tmpvar_8);
  fresnel_3 = tmpvar_9;
  lowp vec4 tmpvar_10;
  tmpvar_10 = texture2D (_ColorControl, vec2(fresnel_3));
  water_2 = tmpvar_10;
  highp vec3 tmpvar_11;
  tmpvar_11 = mix (water_2.xyz, _horizonColor.xyz, water_2.www);
  col_1.xyz = tmpvar_11;
  highp float tmpvar_12;
  tmpvar_12 = _horizonColor.w;
  col_1.w = tmpvar_12;
  gl_FragData[0] = col_1;
}



#endif"
}

SubProgram "flash " {
Keywords { }
Bind "vertex" Vertex
Matrix 0 [glstate_matrix_mvp]
Vector 8 [_WorldSpaceCameraPos]
Matrix 4 [_World2Object]
Vector 9 [unity_Scale]
Float 10 [_WaveScale]
Vector 11 [_WaveOffset]
"agal_vs
c12 0.4 0.45 1.0 0.0
[bc]
aaaaaaaaabaaaiacamaaaakkabaaaaaaaaaaaaaaaaaaaaaa mov r1.w, c12.z
aaaaaaaaabaaahacaiaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r1.xyz, c8
bdaaaaaaaaaaaeacabaaaaoeacaaaaaaagaaaaoeabaaaaaa dp4 r0.z, r1, c6
bdaaaaaaaaaaabacabaaaaoeacaaaaaaaeaaaaoeabaaaaaa dp4 r0.x, r1, c4
bdaaaaaaaaaaacacabaaaaoeacaaaaaaafaaaaoeabaaaaaa dp4 r0.y, r1, c5
adaaaaaaacaaahacaaaaaakeacaaaaaaajaaaappabaaaaaa mul r2.xyz, r0.xyzz, c9.w
acaaaaaaabaaahacacaaaakeacaaaaaaaaaaaaoeaaaaaaaa sub r1.xyz, r2.xyzz, a0
bcaaaaaaaaaaabacabaaaakeacaaaaaaabaaaakeacaaaaaa dp3 r0.x, r1.xyzz, r1.xyzz
akaaaaaaabaaaiacaaaaaaaaacaaaaaaaaaaaaaaaaaaaaaa rsq r1.w, r0.x
aaaaaaaaacaaapacajaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r2, c9
afaaaaaaaaaaaeacacaaaappacaaaaaaaaaaaaaaaaaaaaaa rcp r0.z, r2.w
adaaaaaaaaaaadacaaaaaaoiaaaaaaaaakaaaaaaabaaaaaa mul r0.xy, a0.xzzw, c10.x
adaaaaaaaaaaapacaaaaaaeeacaaaaaaaaaaaakkacaaaaaa mul r0, r0.xyxy, r0.z
abaaaaaaaaaaapacaaaaaaoeacaaaaaaalaaaaoeabaaaaaa add r0, r0, c11
adaaaaaaacaaahaeabaaaappacaaaaaaabaaaafiacaaaaaa mul v2.xyz, r1.w, r1.xzyy
adaaaaaaaaaaadaeaaaaaafeacaaaaaaamaaaaoeabaaaaaa mul v0.xy, r0.xyyy, c12
aaaaaaaaabaaadaeaaaaaaklacaaaaaaaaaaaaaaaaaaaaaa mov v1.xy, r0.wzzz
bdaaaaaaaaaaaiadaaaaaaoeaaaaaaaaadaaaaoeabaaaaaa dp4 o0.w, a0, c3
bdaaaaaaaaaaaeadaaaaaaoeaaaaaaaaacaaaaoeabaaaaaa dp4 o0.z, a0, c2
bdaaaaaaaaaaacadaaaaaaoeaaaaaaaaabaaaaoeabaaaaaa dp4 o0.y, a0, c1
bdaaaaaaaaaaabadaaaaaaoeaaaaaaaaaaaaaaoeabaaaaaa dp4 o0.x, a0, c0
aaaaaaaaaaaaamaeaaaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov v0.zw, c0
aaaaaaaaabaaamaeaaaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov v1.zw, c0
aaaaaaaaacaaaiaeaaaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov v2.w, c0
"
}

SubProgram "d3d11_9x " {
Keywords { }
Bind "vertex" Vertex
ConstBuffer "$Globals" 80 // 80 used size, 5 vars
Float 48 [_WaveScale]
Vector 64 [_WaveOffset] 4
ConstBuffer "UnityPerCamera" 128 // 76 used size, 8 vars
Vector 64 [_WorldSpaceCameraPos] 3
ConstBuffer "UnityPerDraw" 336 // 336 used size, 6 vars
Matrix 0 [glstate_matrix_mvp] 4
Matrix 256 [_World2Object] 4
Vector 320 [unity_Scale] 4
BindCB "$Globals" 0
BindCB "UnityPerCamera" 1
BindCB "UnityPerDraw" 2
// 18 instructions, 1 temp regs, 0 temp arrays:
// ALU 10 float, 0 int, 0 uint
// TEX 0 (0 load, 0 comp, 0 bias, 0 grad)
// FLOW 1 static, 0 dynamic
"vs_4_0_level_9_3
eefiecedanhhoenaaknpedjcnjjlcfldoomokhlpabaaaaaaleafaaaaaeaaaaaa
daaaaaaaaiacaaaanmaeaaaacmafaaaaebgpgodjnaabaaaanaabaaaaaaacpopp
hiabaaaafiaaaaaaaeaaceaaaaaafeaaaaaafeaaaaaaceaaabaafeaaaaaaadaa
acaaabaaaaaaaaaaabaaaeaaabaaadaaaaaaaaaaacaaaaaaaeaaaeaaaaaaaaaa
acaabaaaafaaaiaaaaaaaaaaaaaaaaaaabacpoppfbaaaaafanaaapkamnmmmmdo
ggggogdoaaaaaaaaaaaaaaaabpaaaaacafaaaaiaaaaaapjaafaaaaadaaaaapia
aaaaiijaabaaaakaagaaaaacabaaabiaamaappkaaeaaaaaeaaaaapiaaaaaoeia
abaaaaiaacaaoekaafaaaaadaaaaadoaaaaaoeiaanaaoekaabaaaaacabaaadoa
aaaaoliaabaaaaacaaaaahiaadaaoekaafaaaaadabaaahiaaaaaffiaajaanika
aeaaaaaeaaaaaliaaiaagikaaaaaaaiaabaakeiaaeaaaaaeaaaaahiaakaanika
aaaakkiaaaaapeiaacaaaaadaaaaahiaaaaaoeiaalaanikaaeaaaaaeaaaaahia
aaaaoeiaamaappkaaaaanijbaiaaaaadaaaaaiiaaaaaoeiaaaaaoeiaahaaaaac
aaaaaiiaaaaappiaafaaaaadacaaahoaaaaappiaaaaaoeiaafaaaaadaaaaapia
aaaaffjaafaaoekaaeaaaaaeaaaaapiaaeaaoekaaaaaaajaaaaaoeiaaeaaaaae
aaaaapiaagaaoekaaaaakkjaaaaaoeiaaeaaaaaeaaaaapiaahaaoekaaaaappja
aaaaoeiaaeaaaaaeaaaaadmaaaaappiaaaaaoekaaaaaoeiaabaaaaacaaaaamma
aaaaoeiappppaaaafdeieefcmmacaaaaeaaaabaaldaaaaaafjaaaaaeegiocaaa
aaaaaaaaafaaaaaafjaaaaaeegiocaaaabaaaaaaafaaaaaafjaaaaaeegiocaaa
acaaaaaabfaaaaaafpaaaaadpcbabaaaaaaaaaaaghaaaaaepccabaaaaaaaaaaa
abaaaaaagfaaaaaddccabaaaabaaaaaagfaaaaaddccabaaaacaaaaaagfaaaaad
hccabaaaadaaaaaagiaaaaacabaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaa
aaaaaaaaegiocaaaacaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaa
acaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaa
aaaaaaaaegiocaaaacaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaa
dcaaaaakpccabaaaaaaaaaaaegiocaaaacaaaaaaadaaaaaapgbpbaaaaaaaaaaa
egaobaaaaaaaaaaadiaaaaaipcaabaaaaaaaaaaaigbibaaaaaaaaaaaagiacaaa
aaaaaaaaadaaaaaaaoaaaaaipcaabaaaaaaaaaaaegaobaaaaaaaaaaapgipcaaa
acaaaaaabeaaaaaaaaaaaaaipcaabaaaaaaaaaaaegaobaaaaaaaaaaaegiocaaa
aaaaaaaaaeaaaaaadiaaaaakdccabaaaabaaaaaaegaabaaaaaaaaaaaaceaaaaa
mnmmmmdoggggogdoaaaaaaaaaaaaaaaadgaaaaafdccabaaaacaaaaaalgapbaaa
aaaaaaaadiaaaaajhcaabaaaaaaaaaaafgifcaaaabaaaaaaaeaaaaaaigibcaaa
acaaaaaabbaaaaaadcaaaaalhcaabaaaaaaaaaaaigibcaaaacaaaaaabaaaaaaa
agiacaaaabaaaaaaaeaaaaaaegacbaaaaaaaaaaadcaaaaalhcaabaaaaaaaaaaa
igibcaaaacaaaaaabcaaaaaakgikcaaaabaaaaaaaeaaaaaaegacbaaaaaaaaaaa
aaaaaaaihcaabaaaaaaaaaaaegacbaaaaaaaaaaaigibcaaaacaaaaaabdaaaaaa
dcaaaaalhcaabaaaaaaaaaaaegacbaaaaaaaaaaapgipcaaaacaaaaaabeaaaaaa
igbbbaiaebaaaaaaaaaaaaaabaaaaaahicaabaaaaaaaaaaaegacbaaaaaaaaaaa
egacbaaaaaaaaaaaeeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaadiaaaaah
hccabaaaadaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaadoaaaaabejfdeheo
eiaaaaaaacaaaaaaaiaaaaaadiaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apapaaaaebaaaaaaaaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaaaaaafaepfdej
feejepeoaaeoepfcenebemaaepfdeheoiaaaaaaaaeaaaaaaaiaaaaaagiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaheaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaadamaaaaheaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
adamaaaaheaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaahaiaaaafdfgfpfa
epfdejfeejepeoaafeeffiedepepfceeaaklklkl"
}

}
Program "fp" {
// Fragment combos: 1
//   opengl - ALU: 21 to 21, TEX: 3 to 3
//   d3d9 - ALU: 22 to 22, TEX: 3 to 3
//   d3d11 - ALU: 6 to 6, TEX: 3 to 3, FLOW: 1 to 1
//   d3d11_9x - ALU: 6 to 6, TEX: 3 to 3, FLOW: 1 to 1
SubProgram "opengl " {
Keywords { }
Vector 0 [_horizonColor]
SetTexture 0 [_BumpMap] 2D
SetTexture 1 [_ColorControl] 2D
"!!ARBfp1.0
OPTION ARB_precision_hint_fastest;
# 21 ALU, 3 TEX
PARAM c[2] = { program.local[0],
		{ 2, 1, 0.5 } };
TEMP R0;
TEMP R1;
TEX R0.yw, fragment.texcoord[1], texture[0], 2D;
TEX R1.yw, fragment.texcoord[0], texture[0], 2D;
MAD R0.xy, R0.wyzw, c[1].x, -c[1].y;
MAD R1.xy, R1.wyzw, c[1].x, -c[1].y;
MUL R0.w, R0.y, R0.y;
MUL R0.z, R1.y, R1.y;
MAD R0.w, -R0.x, R0.x, -R0;
MAD R0.z, -R1.x, R1.x, -R0;
ADD R0.w, R0, c[1].y;
RSQ R1.z, R0.w;
ADD R0.z, R0, c[1].y;
RSQ R0.w, R0.z;
RCP R0.z, R1.z;
RCP R1.z, R0.w;
ADD R0.xyz, R1, R0;
MUL R0.xyz, R0, c[1].z;
DP3 R0.x, fragment.texcoord[2], R0;
MOV result.color.w, c[0];
TEX R0, R0.x, texture[1], 2D;
ADD R1.xyz, -R0, c[0];
MAD result.color.xyz, R0.w, R1, R0;
END
# 21 instructions, 2 R-regs
"
}

SubProgram "d3d9 " {
Keywords { }
Vector 0 [_horizonColor]
SetTexture 0 [_BumpMap] 2D
SetTexture 1 [_ColorControl] 2D
"ps_2_0
; 22 ALU, 3 TEX
dcl_2d s0
dcl_2d s1
def c1, 2.00000000, -1.00000000, 1.00000000, 0.50000000
dcl t0.xy
dcl t1.xy
dcl t2.xyz
texld r1, t1, s0
texld r0, t0, s0
mov r0.x, r0.w
mad_pp r3.xy, r0, c1.x, c1.y
mov r1.x, r1.w
mad_pp r2.xy, r1, c1.x, c1.y
mul_pp r1.x, r2.y, r2.y
mul_pp r0.x, r3.y, r3.y
mad_pp r1.x, -r2, r2, -r1
mad_pp r0.x, -r3, r3, -r0
add_pp r1.x, r1, c1.z
rsq_pp r1.x, r1.x
add_pp r0.x, r0, c1.z
rsq_pp r0.x, r0.x
rcp_pp r2.z, r1.x
rcp_pp r3.z, r0.x
add_pp r0.xyz, r3, r2
mul_pp r0.xyz, r0, c1.w
dp3 r0.x, t2, r0
mov r0.xy, r0.x
texld r0, r0, s1
add_pp r1.xyz, -r0, c0
mad_pp r0.xyz, r0.w, r1, r0
mov_pp r0.w, c0
mov_pp oC0, r0
"
}

SubProgram "d3d11 " {
Keywords { }
ConstBuffer "$Globals" 80 // 32 used size, 5 vars
Vector 16 [_horizonColor] 4
BindCB "$Globals" 0
SetTexture 0 [_BumpMap] 2D 0
SetTexture 1 [_ColorControl] 2D 1
// 18 instructions, 2 temp regs, 0 temp arrays:
// ALU 6 float, 0 int, 0 uint
// TEX 3 (0 load, 0 comp, 0 bias, 0 grad)
// FLOW 1 static, 0 dynamic
"ps_4_0
eefieceddebmddfgienjdaabdejnephhglmdjapnabaaaaaaoiadaaaaadaaaaaa
cmaaaaaaleaaaaaaoiaaaaaaejfdeheoiaaaaaaaaeaaaaaaaiaaaaaagiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaheaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaadadaaaaheaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
adadaaaaheaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaahahaaaafdfgfpfa
epfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheocmaaaaaaabaaaaaa
aiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapaaaaaafdfgfpfe
gbhcghgfheaaklklfdeieefcpiacaaaaeaaaaaaaloaaaaaafjaaaaaeegiocaaa
aaaaaaaaacaaaaaafkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaa
fibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaaabaaaaaaffffaaaa
gcbaaaaddcbabaaaabaaaaaagcbaaaaddcbabaaaacaaaaaagcbaaaadhcbabaaa
adaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaacacaaaaaaefaaaaajpcaabaaa
aaaaaaaaegbabaaaabaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaap
dcaabaaaaaaaaaaahgapbaaaaaaaaaaaaceaaaaaaaaaaaeaaaaaaaeaaaaaaaaa
aaaaaaaaaceaaaaaaaaaialpaaaaialpaaaaaaaaaaaaaaaadcaaaaakicaabaaa
aaaaaaaaakaabaiaebaaaaaaaaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadp
dcaaaaakicaabaaaaaaaaaaabkaabaiaebaaaaaaaaaaaaaabkaabaaaaaaaaaaa
dkaabaaaaaaaaaaaelaaaaafecaabaaaaaaaaaaadkaabaaaaaaaaaaaefaaaaaj
pcaabaaaabaaaaaaegbabaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaa
dcaaaaapdcaabaaaabaaaaaahgapbaaaabaaaaaaaceaaaaaaaaaaaeaaaaaaaea
aaaaaaaaaaaaaaaaaceaaaaaaaaaialpaaaaialpaaaaaaaaaaaaaaaadcaaaaak
icaabaaaaaaaaaaaakaabaiaebaaaaaaabaaaaaaakaabaaaabaaaaaaabeaaaaa
aaaaiadpdcaaaaakicaabaaaaaaaaaaabkaabaiaebaaaaaaabaaaaaabkaabaaa
abaaaaaadkaabaaaaaaaaaaaelaaaaafecaabaaaabaaaaaadkaabaaaaaaaaaaa
aaaaaaahhcaabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaaabaaaaaadiaaaaak
hcaabaaaaaaaaaaaegacbaaaaaaaaaaaaceaaaaaaaaaaadpaaaaaadpaaaaaadp
aaaaaaaabaaaaaahbcaabaaaaaaaaaaaegbcbaaaadaaaaaaegacbaaaaaaaaaaa
efaaaaajpcaabaaaaaaaaaaaagaabaaaaaaaaaaaeghobaaaabaaaaaaaagabaaa
abaaaaaaaaaaaaajhcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaaegiccaaa
aaaaaaaaabaaaaaadcaaaaajhccabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaadgaaaaagiccabaaaaaaaaaaadkiacaaaaaaaaaaa
abaaaaaadoaaaaab"
}

SubProgram "gles " {
Keywords { }
"!!GLES"
}

SubProgram "glesdesktop " {
Keywords { }
"!!GLES"
}

SubProgram "flash " {
Keywords { }
Vector 0 [_horizonColor]
SetTexture 0 [_BumpMap] 2D
SetTexture 1 [_ColorControl] 2D
"agal_ps
c1 2.0 -1.0 1.0 0.5
[bc]
ciaaaaaaabaaapacabaaaaoeaeaaaaaaaaaaaaaaafaababb tex r1, v1, s0 <2d wrap linear point>
ciaaaaaaaaaaapacaaaaaaoeaeaaaaaaaaaaaaaaafaababb tex r0, v0, s0 <2d wrap linear point>
aaaaaaaaaaaaabacaaaaaappacaaaaaaaaaaaaaaaaaaaaaa mov r0.x, r0.w
adaaaaaaadaaadacaaaaaafeacaaaaaaabaaaaaaabaaaaaa mul r3.xy, r0.xyyy, c1.x
abaaaaaaadaaadacadaaaafeacaaaaaaabaaaaffabaaaaaa add r3.xy, r3.xyyy, c1.y
aaaaaaaaabaaabacabaaaappacaaaaaaaaaaaaaaaaaaaaaa mov r1.x, r1.w
adaaaaaaacaaadacabaaaafeacaaaaaaabaaaaaaabaaaaaa mul r2.xy, r1.xyyy, c1.x
abaaaaaaacaaadacacaaaafeacaaaaaaabaaaaffabaaaaaa add r2.xy, r2.xyyy, c1.y
adaaaaaaabaaabacacaaaaffacaaaaaaacaaaaffacaaaaaa mul r1.x, r2.y, r2.y
adaaaaaaaaaaabacadaaaaffacaaaaaaadaaaaffacaaaaaa mul r0.x, r3.y, r3.y
bfaaaaaaacaaaiacacaaaaaaacaaaaaaaaaaaaaaaaaaaaaa neg r2.w, r2.x
adaaaaaaacaaaiacacaaaappacaaaaaaacaaaaaaacaaaaaa mul r2.w, r2.w, r2.x
acaaaaaaabaaabacacaaaappacaaaaaaabaaaaaaacaaaaaa sub r1.x, r2.w, r1.x
bfaaaaaaadaaaiacadaaaaaaacaaaaaaaaaaaaaaaaaaaaaa neg r3.w, r3.x
adaaaaaaadaaaiacadaaaappacaaaaaaadaaaaaaacaaaaaa mul r3.w, r3.w, r3.x
acaaaaaaaaaaabacadaaaappacaaaaaaaaaaaaaaacaaaaaa sub r0.x, r3.w, r0.x
abaaaaaaabaaabacabaaaaaaacaaaaaaabaaaakkabaaaaaa add r1.x, r1.x, c1.z
akaaaaaaabaaabacabaaaaaaacaaaaaaaaaaaaaaaaaaaaaa rsq r1.x, r1.x
abaaaaaaaaaaabacaaaaaaaaacaaaaaaabaaaakkabaaaaaa add r0.x, r0.x, c1.z
akaaaaaaaaaaabacaaaaaaaaacaaaaaaaaaaaaaaaaaaaaaa rsq r0.x, r0.x
afaaaaaaacaaaeacabaaaaaaacaaaaaaaaaaaaaaaaaaaaaa rcp r2.z, r1.x
afaaaaaaadaaaeacaaaaaaaaacaaaaaaaaaaaaaaaaaaaaaa rcp r3.z, r0.x
abaaaaaaaaaaahacadaaaakeacaaaaaaacaaaakeacaaaaaa add r0.xyz, r3.xyzz, r2.xyzz
adaaaaaaaaaaahacaaaaaakeacaaaaaaabaaaappabaaaaaa mul r0.xyz, r0.xyzz, c1.w
bcaaaaaaaaaaabacacaaaaoeaeaaaaaaaaaaaakeacaaaaaa dp3 r0.x, v2, r0.xyzz
aaaaaaaaaaaaadacaaaaaaaaacaaaaaaaaaaaaaaaaaaaaaa mov r0.xy, r0.x
ciaaaaaaaaaaapacaaaaaafeacaaaaaaabaaaaaaafaababb tex r0, r0.xyyy, s1 <2d wrap linear point>
bfaaaaaaabaaahacaaaaaakeacaaaaaaaaaaaaaaaaaaaaaa neg r1.xyz, r0.xyzz
abaaaaaaabaaahacabaaaakeacaaaaaaaaaaaaoeabaaaaaa add r1.xyz, r1.xyzz, c0
adaaaaaaabaaahacaaaaaappacaaaaaaabaaaakeacaaaaaa mul r1.xyz, r0.w, r1.xyzz
abaaaaaaaaaaahacabaaaakeacaaaaaaaaaaaakeacaaaaaa add r0.xyz, r1.xyzz, r0.xyzz
aaaaaaaaaaaaaiacaaaaaaoeabaaaaaaaaaaaaaaaaaaaaaa mov r0.w, c0
aaaaaaaaaaaaapadaaaaaaoeacaaaaaaaaaaaaaaaaaaaaaa mov o0, r0
"
}

SubProgram "d3d11_9x " {
Keywords { }
ConstBuffer "$Globals" 80 // 32 used size, 5 vars
Vector 16 [_horizonColor] 4
BindCB "$Globals" 0
SetTexture 0 [_BumpMap] 2D 0
SetTexture 1 [_ColorControl] 2D 1
// 18 instructions, 2 temp regs, 0 temp arrays:
// ALU 6 float, 0 int, 0 uint
// TEX 3 (0 load, 0 comp, 0 bias, 0 grad)
// FLOW 1 static, 0 dynamic
"ps_4_0_level_9_3
eefiecedkofmlfmpjkkgbapakgmdpiifcipcecjiabaaaaaalmafaaaaaeaaaaaa
daaaaaaaaaacaaaaaaafaaaaiiafaaaaebgpgodjmiabaaaamiabaaaaaaacpppp
jaabaaaadiaaaaaaabaacmaaaaaadiaaaaaadiaaacaaceaaaaaadiaaaaaaaaaa
abababaaaaaaabaaabaaaaaaaaaaaaaaabacppppfbaaaaafabaaapkaaaaaaaea
aaaaialpaaaaiadpaaaaaadpbpaaaaacaaaaaaiaaaaaadlabpaaaaacaaaaaaia
abaaadlabpaaaaacaaaaaaiaacaaahlabpaaaaacaaaaaajaaaaiapkabpaaaaac
aaaaaajaabaiapkaecaaaaadaaaacpiaabaaoelaaaaioekaecaaaaadabaacpia
aaaaoelaaaaioekaaeaaaaaeabaacdiaabaaohiaabaaaakaabaaffkaaeaaaaae
abaaciiaabaaaaiaabaaaaibabaakkkaaeaaaaaeabaaciiaabaaffiaabaaffib
abaappiaahaaaaacabaaciiaabaappiaagaaaaacabaaceiaabaappiaaeaaaaae
aaaacdiaaaaaohiaabaaaakaabaaffkaaeaaaaaeaaaaciiaaaaaaaiaaaaaaaib
abaakkkaaeaaaaaeaaaaciiaaaaaffiaaaaaffibaaaappiaahaaaaacaaaaciia
aaaappiaagaaaaacaaaaceiaaaaappiaacaaaaadaaaachiaaaaaoeiaabaaoeia
afaaaaadaaaachiaaaaaoeiaabaappkaaiaaaaadaaaacdiaacaaoelaaaaaoeia
ecaaaaadaaaacpiaaaaaoeiaabaioekabcaaaaaeabaachiaaaaappiaaaaaoeka
aaaaoeiaabaaaaacabaaciiaaaaappkaabaaaaacaaaicpiaabaaoeiappppaaaa
fdeieefcpiacaaaaeaaaaaaaloaaaaaafjaaaaaeegiocaaaaaaaaaaaacaaaaaa
fkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaafibiaaaeaahabaaa
aaaaaaaaffffaaaafibiaaaeaahabaaaabaaaaaaffffaaaagcbaaaaddcbabaaa
abaaaaaagcbaaaaddcbabaaaacaaaaaagcbaaaadhcbabaaaadaaaaaagfaaaaad
pccabaaaaaaaaaaagiaaaaacacaaaaaaefaaaaajpcaabaaaaaaaaaaaegbabaaa
abaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaapdcaabaaaaaaaaaaa
hgapbaaaaaaaaaaaaceaaaaaaaaaaaeaaaaaaaeaaaaaaaaaaaaaaaaaaceaaaaa
aaaaialpaaaaialpaaaaaaaaaaaaaaaadcaaaaakicaabaaaaaaaaaaaakaabaia
ebaaaaaaaaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadpdcaaaaakicaabaaa
aaaaaaaabkaabaiaebaaaaaaaaaaaaaabkaabaaaaaaaaaaadkaabaaaaaaaaaaa
elaaaaafecaabaaaaaaaaaaadkaabaaaaaaaaaaaefaaaaajpcaabaaaabaaaaaa
egbabaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaapdcaabaaa
abaaaaaahgapbaaaabaaaaaaaceaaaaaaaaaaaeaaaaaaaeaaaaaaaaaaaaaaaaa
aceaaaaaaaaaialpaaaaialpaaaaaaaaaaaaaaaadcaaaaakicaabaaaaaaaaaaa
akaabaiaebaaaaaaabaaaaaaakaabaaaabaaaaaaabeaaaaaaaaaiadpdcaaaaak
icaabaaaaaaaaaaabkaabaiaebaaaaaaabaaaaaabkaabaaaabaaaaaadkaabaaa
aaaaaaaaelaaaaafecaabaaaabaaaaaadkaabaaaaaaaaaaaaaaaaaahhcaabaaa
aaaaaaaaegacbaaaaaaaaaaaegacbaaaabaaaaaadiaaaaakhcaabaaaaaaaaaaa
egacbaaaaaaaaaaaaceaaaaaaaaaaadpaaaaaadpaaaaaadpaaaaaaaabaaaaaah
bcaabaaaaaaaaaaaegbcbaaaadaaaaaaegacbaaaaaaaaaaaefaaaaajpcaabaaa
aaaaaaaaagaabaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaaaaaaaaaj
hcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaaegiccaaaaaaaaaaaabaaaaaa
dcaaaaajhccabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaabaaaaaaegacbaaa
aaaaaaaadgaaaaagiccabaaaaaaaaaaadkiacaaaaaaaaaaaabaaaaaadoaaaaab
ejfdeheoiaaaaaaaaeaaaaaaaiaaaaaagiaaaaaaaaaaaaaaabaaaaaaadaaaaaa
aaaaaaaaapaaaaaaheaaaaaaaaaaaaaaaaaaaaaaadaaaaaaabaaaaaaadadaaaa
heaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaaadadaaaaheaaaaaaacaaaaaa
aaaaaaaaadaaaaaaadaaaaaaahahaaaafdfgfpfaepfdejfeejepeoaafeeffied
epepfceeaaklklklepfdeheocmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaa
aaaaaaaaadaaaaaaaaaaaaaaapaaaaaafdfgfpfegbhcghgfheaaklkl"
}

}

#LINE 85

	}
}

// -----------------------------------------------------------
//  Old cards

// three texture, cubemaps
Subshader {
	Tags { "RenderType"="Opaque" }
	Pass {
		Color (0.5,0.5,0.5,0.5)
		SetTexture [_MainTex] {
			Matrix [_WaveMatrix]
			combine texture * primary
		}
		SetTexture [_MainTex] {
			Matrix [_WaveMatrix2]
			combine texture * primary + previous
		}
		SetTexture [_ColorControlCube] {
			combine texture +- previous, primary
			Matrix [_Reflection]
		}
	}
}

// dual texture, cubemaps
Subshader {
	Tags { "RenderType"="Opaque" }
	Pass {
		Color (0.5,0.5,0.5,0.5)
		SetTexture [_MainTex] {
			Matrix [_WaveMatrix]
			combine texture
		}
		SetTexture [_ColorControlCube] {
			combine texture +- previous, primary
			Matrix [_Reflection]
		}
	}
}

// single texture
Subshader {
	Tags { "RenderType"="Opaque" }
	Pass {
		Color (0.5,0.5,0.5,0)
		SetTexture [_MainTex] {
			Matrix [_WaveMatrix]
			combine texture, primary
		}
	}
}

}
