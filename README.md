<div align="center">

<img src="docs/img/portada.jpg" alt="Alucinesia" width="560">

# Alucinesia

**Se acabaron tus pastillas. Y alguien está tocando la puerta.**

*Horror psicológico en primera persona con estética PSX.*

[![Jugar en itch.io](https://img.shields.io/badge/Jugar-itch.io-FA5C5C?style=for-the-badge&logo=itchdotio&logoColor=white)](https://thevacuumgentleman.itch.io/alucinesia)
[![GDD](https://img.shields.io/badge/GDD-Canva-00C4CC?style=for-the-badge&logo=canva&logoColor=white)](https://canva.link/e1261sfii8lck8m)

![Unity](https://img.shields.io/badge/Unity-6000.3.25f1-222c37?logo=unity)
![URP](https://img.shields.io/badge/Render-URP-4c8eda)
![Input System](https://img.shields.io/badge/Input-New%20Input%20System-6f42c1)
![Plataforma](https://img.shields.io/badge/Plataforma-PC%20%7C%20Web-2ea44f)
![Jam](https://img.shields.io/badge/Game%20Jam-2%20Mundos-D7263D)

</div>

---

## Sobre el juego

Despiertas solo en tu casa, a mitad de la noche, y descubres que tus frascos de pastillas están vacíos. Sin tu medicación, la frontera entre la realidad y tus delirios empieza a romperse. Tienes que mantener la calma, revisar cada rincón y decidir en qué confiar: en lo que ves o en lo que sabes.

| | |
|---|---|
| **Género** | Horror psicológico en primera persona |
| **Jugadores** | 1 |
| **Duración** | 20 a 30 minutos |
| **Estética** | PSX / low poly con iluminación cálida y post-procesado |
| **Idioma** | Español |
| **Recomendado** | Audífonos y pantalla completa |

## Dos mundos

La temática de la jam fue **"2 mundos"**. En Alucinesia los dos mundos son **la realidad** y **las alucinaciones** del protagonista. A medida que sube la ansiedad, la casa cambia, los sonidos se distorsionan y lo que te persigue se vuelve cada vez más real. Respirar a tiempo es lo único que te devuelve el control.

<p align="center">
  <img src="docs/img/logo.jpg" width="260" alt="Logo de Alucinesia: un cerebro partido entre realidad y alucinación">
</p>

## Cómo se juega

- **Busca tus frascos.** Explora la casa, abre puertas y cajones y examina cada frasco que encuentres.
- **Respira.** En los momentos de crisis aparece un ejercicio de respiración; cuanto más avanza la noche, más difícil es mantener el ritmo.
- **Apaga lo que se enciende solo.** Televisores, secadora, lavadora y monitores se activan sin explicación; el ruido no te deja pensar.
- **Construye una barricada.** Rompe muebles, carga las tablas de una en una y bloquea la puerta antes de que entre.
- **Cuida tu luz.** La linterna tiene una sola pila y la vela se apaga con el tiempo y con los sustos; las cerillas están escondidas por la casa.
- **Sobrevive al sótano.** Algo te escucha en la oscuridad. Si te atrapa, vuelves al último punto de control.
- **Decide.** Al final eliges entre buscar el último frasco o tomar el martillo. Hay dos desenlaces.

<p align="center">
  <img src="docs/img/sala.jpg" width="49%" alt="Sala de la casa">
  <img src="docs/img/estudio.jpg" width="49%" alt="Estudio con el televisor">
</p>
<p align="center">
  <img src="docs/img/cocina.jpg" width="49%" alt="Comedor a oscuras">
  <img src="docs/img/escaleras.jpg" width="49%" alt="Escaleras al segundo piso">
</p>
<p align="center">
  <img src="docs/img/enemigo.jpg" width="70%" alt="La figura en el sótano">
</p>

## Controles

| Tecla | Acción |
|---|---|
| `W A S D` | Moverse |
| `Ratón` | Mirar |
| `Shift` | Correr |
| `C` | Agacharse |
| `E` | Interactuar / recoger / cambiar de luz |
| `F` | Encender o apagar la linterna o la vela |
| `Espacio` | Respirar (durante el ejercicio de respiración) |
| `P` | Pausa |

## Equipo QuadBurger

| Integrante | Rol |
|---|---|
| **Juan Sebastián Gravini Contreras** | Project manager, programador y artista de iluminación |
| **Sergio Alejandro Carmona Delgado** | Diseñador de audio y UI |
| **Kimberly Rojas Bonilla** | Level designer y UI |
| **Gustavo Adolfo Parra Castro** | Concept art y documentación |

## Documentación

- **Página del juego:** [itch.io](https://thevacuumgentleman.itch.io/alucinesia)
- **GDD:** [Canva](https://canva.link/e1261sfii8lck8m)

## Abrir el proyecto

**Requisitos:** Unity **6000.3.25f1** (con el módulo de WebGL si quieres compilar para web) y [Git LFS](https://git-lfs.com/), porque audio, texturas y fuentes se guardan con LFS.

```bash
git lfs install
git clone https://github.com/Vacuumgentleman/GameJamGenV2G1.git
```

1. Abre la carpeta del repositorio desde Unity Hub.
2. Abre `Assets/GameJamG1/Scenes/Menus/Main Menu.unity`.
3. Pulsa **Play**.

Escenas del build, en orden: `Main Menu` y `Prototype` (la casa completa).

## Arquitectura

El código vive en `Assets/GameJamG1/Scripts` y está organizado por sistemas. Todo se configura desde el inspector: diálogos, flags de historia, tiempos, sonidos y niveles de alucinación.

```
Scripts/
├── Player/          Movimiento en primera persona, cámara, pasos y lector único de entrada
├── Interaction/     Interfaz IInteractable, outline al apuntar y prompts
├── Objects/         Puertas, cajones, objetos examinables y elección final exclusiva
├── Items/           Dos manos (luz y carga), linterna, vela, cerillas y ItemData
├── Dialogue/        Monólogo interno con efecto máquina de escribir y sonido por letra
├── Story/           StoryFlag, StoryManager, StoryTrigger y StoryReaction
├── Hallucination/   Niveles de alucinación (1 a 3) y minijuego de respiración
├── Barricade/       Muebles rompibles y puntos de barricada con vista previa
├── Enemy/           Monstruo con NavMesh, ruido, persecución y rotura de puertas
├── Sequences/       Mirilla, secuencias temporizadas, fundidos y pantallas de final
├── Gameplay/        Checkpoints y muerte, aparatos eléctricos, luces y ambiente
├── Audio/ UI/       Menú principal, pausa, mezclador y sonidos de botones
└── Editor/          Menús para montar la historia, hornear el NavMesh y crear prefabs
```

**Decisiones clave**

- **Historia guiada por datos.** El progreso se guarda en `StoryFlag` (ScriptableObjects). Los `StoryTrigger` y `StoryReaction` reaccionan a flags, zonas o diálogos y disparan resultados (diálogos, nivel de alucinación, enemigos, checkpoints, finales) sin escribir código por escena.
- **Diálogos como assets.** Cada monólogo es un `DialogueSequence`, editable por cualquiera del equipo.
- **Alucinación por perfiles.** `HallucinationLevelProfile` define el post-procesado, la distorsión de cámara y el audio ambiente de cada nivel, y la dificultad de la respiración escala con ese nivel.
- **Un solo lector de entrada.** `PlayerInputReader` encapsula el New Input System; el resto de sistemas consume sus eventos.
- **Interacción uniforme.** Todo lo que se puede usar hereda de `InteractableBase` e implementa `IInteractable`, con resaltado y mensajes de bloqueo.
- **Herramientas de editor.** `GameJam > Conectar historia` monta las reacciones de la escena y `GameJam > Hornear NavMesh` prepara la navegación del monstruo.

## Créditos de assets

Escenarios y props con estética PSX (casa abandonada, sala de estar y cielos PSX), fuente *Eater* de Google Fonts y efectos de sonido de librerías gratuitas. Todos los derechos de estos assets pertenecen a sus autores.

---

<div align="center">
Hecho por <b>QuadBurger</b> en la Game Jam de Generation.
</div>
