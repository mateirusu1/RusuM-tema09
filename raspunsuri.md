Răspunsuri – Texturare în OpenGL
1. Utilizarea imaginilor cu transparență și fără

Imagini fără transparență (RGB):
Textura este aplicată complet pe obiect, toate pixel-urile acoperă suprafața obiectului. Nu există porțiuni „invizibile”.

Imagini cu transparență (RGBA):
Canalul alpha permite ca anumite părți ale texturii să fie transparente. Astfel, se poate vedea obiectul sau scena din spate prin zonele transparente.

Observație: pentru a folosi transparența, este nevoie de activarea blending-ului în OpenGL:

GL.Enable(GL_BLEND);
GL.BlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

2. Formate de imagine aplicabile în texturare OpenGL

Formatele cel mai des folosite:

BMP – simplu, fără compresie, fără transparență nativă

PNG – suportă transparență (alpha)

TGA – suportă transparență, folosit frecvent în aplicații grafice

JPEG – fără transparență, cu compresie

Practic, orice format care poate fi încărcat în memorie ca bitmap poate fi convertit într-o textură OpenGL.

3. Efectul modificării culorii obiectului texturat (prin RGB)

Modificarea canalelor RGB ale obiectului poate influența modul în care textura este afișată:

Dacă culoarea obiectului este albă (1,1,1), textura se afișează original, fără modificări.

Dacă culoarea obiectului este modificată (ex: roșu (1,0,0)), textura va fi modulată cu această culoare: canalele roșu/verde/albastru ale texturii se înmulțesc cu valorile RGB ale obiectului.

Practic, se poate obține efectul de „tint” sau de înălțare/scădere a luminozității texturii.

4. Diferențe între scena cu obiecte texturate și iluminare activată vs. dezactivată

Iluminare activată:

Obiectele texturate reacționează la lumină: umbre, reflexii, intensitatea și culoarea luminii afectează modul în care textura se vede.

Textura poate părea mai realistă, cu efecte de relief sau shading.

Iluminare dezactivată:

Textura este afișată „plan”, fără efecte de lumină.

Culorile texturii rămân constante, indiferent de poziția sau intensitatea luminii.

Scenele pot părea plate și mai puțin realiste, dar textura este vizibilă exact cum a fost creată.