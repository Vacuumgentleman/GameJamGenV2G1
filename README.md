<div align="center">

<img src="docs/img/hero.jpg" alt="Alucinesia: se acabaron tus pastillas. Horror psicológico en primera persona con estética PSX." width="100%">

<br>

<a href="https://thevacuumgentleman.itch.io/alucinesia"><img src="https://img.shields.io/badge/JUGAR_EN_ITCH.IO-D7263D?style=for-the-badge&logo=itchdotio&logoColor=white" alt="Jugar en itch.io"></a>
<a href="https://canva.link/e1261sfii8lck8m"><img src="https://img.shields.io/badge/GDD-1C1214?style=for-the-badge&logo=canva&logoColor=4CC9F0" alt="GDD en Canva"></a>

<img src="https://img.shields.io/badge/Unity-6000.3-1C1214?style=flat-square&logo=unity&logoColor=EDE3E3" alt="Unity 6000.3">
<img src="https://img.shields.io/badge/URP-1C1214?style=flat-square" alt="URP">
<img src="https://img.shields.io/badge/PC_·_Web-1C1214?style=flat-square" alt="PC y Web">
<img src="https://img.shields.io/badge/Jam-Dos_Mundos-D7263D?style=flat-square" alt="Categoría Dos Mundos">
<img src="https://img.shields.io/badge/Equipo-QuadBurger-4CC9F0?style=flat-square&labelColor=1C1214" alt="Equipo QuadBurger">

<br><br>

<img src="docs/img/pitch.jpg" alt="Una noche. Sin pastillas. Alguien toca la puerta. Una persona que vive sola sufre alucinaciones severas y se queda sin su medicación; tiene una noche para encontrarla antes de cometer un error irreversible." width="100%">

<img src="docs/img/h_pilares.jpg" alt="Pilares de diseño" width="100%">
<img src="docs/img/pilares.jpg" alt="Narrativo: monólogo interno, ataques de pánico y un dilema moral. Mecánico: exploración tensa con recursos limitados. Estético: estética PSX rústica y en ruinas." width="100%">

<img src="docs/img/h_mundos.jpg" alt="Dos mundos" width="100%">
<img src="docs/img/dos_mundos.jpg" alt="Realidad: tu casa, tus recuerdos y tus pastillas. Alucinación: sube con el estrés, la casa se deforma y algo te persigue." width="100%">

<img src="docs/img/h_estructura.jpg" alt="Estructura de la noche" width="100%">
<img src="docs/img/estructura.jpg" alt="1 El despertar, 2 Sobrecarga, 3 Barricada, 4 Persecución, 5 Clímax dual." width="100%">

<img src="docs/img/h_mecanicas.jpg" alt="Mecánicas principales" width="100%">
<img src="docs/img/mecanicas.jpg" alt="Respiración, iluminación, barricada, ruido, alucinación y exploración." width="100%">

<img src="docs/img/h_sotano.jpg" alt="Persecución en el sótano" width="100%">
<img src="docs/img/enemigo_estados.jpg" alt="Un laberinto de cajas con un enemigo que escucha cada paso. Estados: inactivo, patrulla, investiga, persigue." width="100%">

<img src="docs/img/h_finales.jpg" alt="Una decisión, dos finales" width="100%">
<img src="docs/img/finales.jpg" alt="Final malo: tomas el martillo por pánico. Final bueno: eliges el último frasco y la psicosis se disipa." width="100%">

<img src="docs/img/h_mapa.jpg" alt="Mapa de la casa" width="100%">
<img src="docs/img/mapa.jpg" alt="Planos del exterior, primer piso, segundo piso y ático, y sótano laberinto." width="100%">

<img src="docs/img/h_galeria.jpg" alt="La casa" width="100%">

<img src="docs/img/sala.jpg" width="49%" alt="Sala"> <img src="docs/img/estudio.jpg" width="49%" alt="Estudio con el televisor">
<img src="docs/img/cocina.jpg" width="49%" alt="Comedor a oscuras"> <img src="docs/img/escaleras.jpg" width="49%" alt="Escaleras al segundo piso">

<img src="docs/img/h_controles.jpg" alt="Controles" width="100%">
<img src="docs/img/controles.jpg" alt="WASD moverse, ratón mirar, Shift correr, C agacharse, E interactuar, F luz, Espacio respirar, P pausa." width="100%">

<sub>Recomendado: juega con audífonos y en pantalla completa.</sub>

<img src="docs/img/h_equipo.jpg" alt="Equipo QuadBurger" width="100%">
<img src="docs/img/equipo.jpg" alt="Juan Sebastián Gravini Contreras, project manager, programador y artista de iluminación. Sergio Alejandro Carmona Delgado, diseñador de audio y UI. Kimberly Rojas Bonilla, level designer y UI. Gustavo Adolfo Parra Castro, concept art y documentación." width="100%">

<img src="docs/img/h_tecnico.jpg" alt="Bajo el capó" width="100%">

</div>

| Sistema | Qué hace |
|---|---|
| **Banderas de historia** | El progreso vive en `StoryFlag` (ScriptableObjects). `StoryTrigger` y `StoryReaction` reaccionan a flags, zonas o diálogos y disparan diálogos, alucinación, enemigos, checkpoints y finales, todo desde el inspector. |
| **Diálogos** | Cada monólogo es un `DialogueSequence` con efecto máquina de escribir y sonido por letra; no hay texto en el código. |
| **Alucinación** | `HallucinationLevelProfile` define post-procesado, distorsión de cámara y audio de cada nivel (1 a 3); la respiración se vuelve más difícil con el nivel. |
| **Interacción** | Todo lo usable hereda de `InteractableBase`: outline al apuntar, mensajes de bloqueo y dos manos (luz y carga). |
| **Enemigo** | IA con NavMesh que escucha el canal de ruido del jugador, patrulla, investiga, persigue y rompe puertas. |
| **Secuencias** | Mirilla, eventos temporizados, fundidos y pantallas de final. |
| **Herramientas** | `GameJam > Conectar historia` monta las reacciones de la escena; `GameJam > Hornear NavMesh` prepara la navegación. |

<details>
<summary><b>Estructura de carpetas</b></summary>

```
Assets/GameJamG1/Scripts/
├── Player/          Movimiento en primera persona, cámara, pasos y lector único de entrada
├── Interaction/     IInteractable, outline y prompts
├── Objects/         Puertas, cajones, objetos examinables y elección final
├── Items/           Linterna, vela, cerillas, ItemData y manos
├── Dialogue/        Monólogo interno
├── Story/           StoryFlag, StoryManager, StoryTrigger, StoryReaction
├── Hallucination/   Niveles de alucinación y minijuego de respiración
├── Barricade/       Muebles rompibles y puntos de barricada
├── Enemy/           Monstruo, ruido, persecución y rotura de puertas
├── Sequences/       Mirilla, secuencias, fundidos y finales
├── Gameplay/        Checkpoints, aparatos eléctricos, luces y ambiente
├── Audio/ UI/       Menú principal, pausa, mezclador y sonidos de UI
└── Editor/          Menús de montaje de historia, NavMesh y prefabs
```

</details>

<div align="center">
<img src="docs/img/h_jugar.jpg" alt="Jugar y abrir el proyecto" width="100%">
</div>

**Jugar:** [thevacuumgentleman.itch.io/alucinesia](https://thevacuumgentleman.itch.io/alucinesia) · **GDD:** [Canva](https://canva.link/e1261sfii8lck8m)

**Abrir el proyecto** con Unity **6000.3.25f1** y [Git LFS](https://git-lfs.com/) (audio, texturas y fuentes van por LFS):

```bash
git lfs install
git clone https://github.com/Vacuumgentleman/GameJamGenV2G1.git
```

Abre la carpeta desde Unity Hub, carga `Assets/GameJamG1/Scenes/Menus/Main Menu.unity` y pulsa **Play**. El build incluye `Main Menu` y `Prototype` (la casa completa).

<sub>Escenarios y props con estética PSX, fuente <i>Eater</i> de Google Fonts y efectos de sonido de librerías gratuitas; los derechos pertenecen a sus autores.</sub>

<div align="center">
<img src="docs/img/footer.jpg" alt="Alucinesia · QuadBurger · Game Jam Generation · Categoría Dos Mundos" width="100%">
</div>
