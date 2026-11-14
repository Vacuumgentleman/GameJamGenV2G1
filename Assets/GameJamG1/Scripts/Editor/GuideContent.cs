namespace GameJamG1.EditorTools
{
    internal readonly struct GuideTopic
    {
        public readonly string Title;
        public readonly string Body;

        public GuideTopic(string title, string body)
        {
            Title = title;
            Body = body;
        }
    }

    internal readonly struct PrefabGuide
    {
        public readonly string Name;
        public readonly string Summary;
        public readonly string Setup;

        public PrefabGuide(string name, string summary, string setup)
        {
            Name = name;
            Summary = summary;
            Setup = setup;
        }
    }

    internal static class GuideContent
    {
        public const string Overview =
            "Este proyecto se arma arrastrando prefabs a la escena y configurandolos en el Inspector. No hace falta escribir codigo para la mayoria de interacciones.\n\n" +
            "1. Abre la pestana Prefabs, elige un objeto y pulsa 'Agregar a la escena'.\n" +
            "2. Reemplaza el cubo por tu modelo: pon tu mesh como hijo y desactiva o borra el cubo. Conserva el collider.\n" +
            "3. En el Inspector, arriba aparece un resumen en lenguaje normal de lo que hace el objeto. Debajo estan los campos.\n" +
            "4. Para la historia usa la pestana Historia: crea flags y dialogos, y mira en vivo quien pone y quien exige cada flag.\n" +
            "5. Si tienes dudas de como lograr algo, mira la pestana Recetas.";

        public const string StoryConcept =
            "FLAG: un interruptor de historia (por ejemplo 'Despierta' o 'FinalBueno'). Es un asset en ScriptableObjects/Flags. Esta activa o no.\n\n" +
            "QUIEN LA PONE: cualquier interactuable en 'Flags To Set On Interact'; el punto de barricada en 'Completion Flags'; o por codigo con StoryManager.Instance.Set(flag).\n\n" +
            "QUIEN LA EXIGE: cualquier interactuable en 'Required Flags' (queda bloqueado hasta que todas esten activas) o un StoryTrigger.\n\n" +
            "STORY TRIGGER: dispara un dialogo y eventos cuando el jugador entra a una zona, cuando cambia una flag, o cuando algo llama a Trigger().\n\n" +
            "DIALOGO: un asset DialogueSequence con una o varias lineas. Se asigna a interactuables, triggers o se llama desde eventos.\n\n" +
            "UNITYEVENT: cada interactuable y trigger tiene 'On Interact' / 'On Triggered'. Ahi arrastras un objeto y eliges una funcion (por ejemplo GameObject.SetActive) para que algo ocurra en el mundo.";

        public static readonly PrefabGuide[] Prefabs =
        {
            new PrefabGuide("Interactable_Basic", "Objeto que se usa con E y muestra un dialogo.", "Prompt Text, Interact Dialogue, Flags To Set On Interact, Required Flags, One Shot."),
            new PrefabGuide("Examinable", "Objeto que se revisa una sola vez (frascos, notas, fotos).", "Interact Dialogue y Flags To Set On Interact. Ya viene como One Shot."),
            new PrefabGuide("Door", "Puerta con bisagra que abre y cierra.", "Required Flags para bloquearla con la historia, Locked Dialogue para lo que dice al estar bloqueada. Rota sobre su bisagra: coloca el modelo respetando el origen."),
            new PrefabGuide("Drawer", "Cajon o gaveta que se desliza y revela objetos.", "Open Offset (cuanto se desplaza) y Contents (objetos ocultos hasta abrir)."),
            new PrefabGuide("StoryTrigger_Zone", "Zona invisible que dispara dialogo y eventos de historia.", "Required Flags, Mode, activacion (zona o cambio de flag), Fire Once, Dialogue On Trigger y On Triggered."),
            new PrefabGuide("Peephole", "Mirilla: fundido, camara, secuencia de eventos y monologo.", "Peephole Camera, Sequence (TimedEventSequence hijo con los pasos), End Dialogue y Flags To Set On Interact."),
            new PrefabGuide("MatchPickup", "Cerillas sueltas que suman al contador.", "Amount (cuantas cerillas da)."),
            new PrefabGuide("Flashlight_World", "Linterna recogible. F la enciende, dura 120 s.", "Se configura en su ItemData (Flashlight) y en el componente Flashlight."),
            new PrefabGuide("Candle_World", "Vela recogible. F gasta una cerilla, correr la apaga.", "Burn Time Per Match, No Matches Dialogue y Extinguish On Sprint en el componente Candle."),
            new PrefabGuide("Plank_World", "Tabla recogible para barricadas.", "Es el item que consume el punto de barricada."),
            new PrefabGuide("Hammer_World", "Martillo recogible (objeto de mano).", "ItemData Hammer. Se puede usar como eleccion de final."),
            new PrefabGuide("Chair_Breakable", "Mueble que se rompe y suelta tablas.", "Plank Prefab, Plank Count, Spawn Offset y Spread Radius."),
            new PrefabGuide("Door_Barricade", "Puerta con punto de barricada y 3 tablas con previsualizacion.", "Required Planks, Completion Flags, Need Plank Dialogue. La puerta se bloquea con la primera tabla."),
            new PrefabGuide("ScreenFader", "Fundido a negro. Uno por escena.", "Lo usan la mirilla y las secuencias. Se llama con ScreenFader.Instance.FadeOut(seg) o FadeIn(seg)."),
            new PrefabGuide("HallucinationManager", "Nivel de alucinacion 0 a 3 con volumen y sonido ambiente. Uno por escena.", "Profiles (un HallucinationLevelProfile por nivel), Transition Duration. Usa Increase, Decrease o SetLevel desde eventos."),
            new PrefabGuide("BreathingCanvas", "Minijuego de respirar con ESPACIO. Uno por escena.", "Llama a BreathingMinigame.Begin desde cualquier evento. Ver pestana Sistemas para la dificultad por nivel.")
        };

        public static readonly GuideTopic[] Systems =
        {
            new GuideTopic("Controles del jugador",
                "WASD mover. Mouse mirar. Shift correr. C agacharse (mantener). E interactuar. F encender o apagar la luz. ESPACIO inhalar en el minijuego de respirar.\n\n" +
                "Solo se puede llevar 1 objeto en la mano. Recoger otro cambia el actual: el anterior cae al suelo en el lugar del nuevo y conserva su bateria o tiempo de vela."),
            new GuideTopic("Interaccion (InteractableBase)",
                "Todo lo que se usa con E hereda de InteractableBase. Campos:\n" +
                "- Prompt Text: texto bajo la mirilla.\n" +
                "- Required Flags / Locked Dialogue: bloquea el objeto hasta que esten todas las flags. Si Locked Dialogue esta vacio usa el dialogo bloqueado por defecto.\n" +
                "- Interact Dialogue, Flags To Set On Interact, Interact Sound.\n" +
                "- One Shot: se desactiva tras usarse. Hide When Consumed: ademas se oculta.\n" +
                "- On Interact / On Locked Interact: UnityEvents para disparar cosas en el mundo.\n" +
                "Necesita un collider y, para resaltarse, un OutlineHighlight con el material Outline_InvertedHull."),
            new GuideTopic("Puertas y cajones",
                "Door: abre y cierra rotando sobre su bisagra. Estados: cerrada, abierta, bloqueada (por flags) y barricada. Barricada la bloquea con el dialogo Barricaded Dialogue.\n\n" +
                "Drawer: se desliza segun Open Offset. Los objetos en Contents estan ocultos hasta la primera apertura.\n\n" +
                "ExclusiveChoiceGroup: lista de interactuables de los que solo se puede usar uno; al usar uno el resto se desactiva. Sirve para finales exclusivos."),
            new GuideTopic("Items, luz y cerillas",
                "Cada item tiene un ItemData (nombre, prefab de mano, prefab de mundo, sonido y dialogo al recoger) y un prefab _Held y otro _World.\n\n" +
                "Linterna: F enciende, 120 s de bateria, dialogo al agotarse. Vela: F gasta una cerilla y dura 60 s por cerilla; correr la apaga; sin cerillas dice su dialogo. MatchBox (en el jugador) guarda las cerillas y MatchCounterUI las muestra en pantalla. MatchPickup suma cerillas."),
            new GuideTopic("Barricada",
                "BreakableFurniture suelta tablas al romperse. El jugador agarra una tabla y la coloca en el BarricadePoint de la puerta: se muestra una silueta pulsante donde va la siguiente tabla.\n\n" +
                "Con la primera tabla la puerta ya no abre. Al completar Required Planks se ponen las Completion Flags. BarricadePoint tiene TakeDamage listo para que un enemigo rompa tablas (Max Health, On Breached)."),
            new GuideTopic("Secuencias, fundido y mirilla",
                "TimedEventSequence: lista de pasos con retraso. Cada paso tiene un UnityEvent. Play() la inicia; Can Replay y Play On Enable ajustan el comportamiento.\n\n" +
                "PeepholeSequence: al usarla funde a negro, cambia a la camara de la mirilla, corre su secuencia, vuelve, devuelve el control y reproduce End Dialogue.\n\n" +
                "ScreenFader: FadeOut(seg) y FadeIn(seg) desde eventos."),
            new GuideTopic("Ruido y enemigo",
                "NoiseChannel es un asset (Noise_Main) por el que viaja el ruido. NoiseEmitter.Emit() lo emite desde su posicion. El minijuego de respirar emite ruido al inhalar.\n\n" +
                "EnemyController tiene estados Inactivo, Patrulla, Investigar y Perseguir y escucha el canal. Aun no tiene comportamiento: el equipo de enemigo lo extiende sobreescribiendo OnStateEnter, OnStateExit y OnNoiseHeard."),
            new GuideTopic("Alucinacion y respiracion",
                "HallucinationManager guarda el nivel (0 a 3). Cada nivel tiene un HallucinationLevelProfile con su volumen de post-proceso y ambiente. Cambia el nivel con Increase, Decrease o SetLevel.\n\n" +
                "Minijuego de respirar: manten ESPACIO para inhalar y suelta para exhalar. Mantén el circulo dentro de la zona hasta llenar la barra. Exito baja un nivel. Si Time Limit es mayor que 0 y se acaba el tiempo falla y sube un nivel.\n\n" +
                "Dificultad por nivel (lista Difficulty By Level): a mayor nivel la zona es mas angosta, el circulo mas rapido, la zona se desplaza y el circulo es inestable. Retry Cooldown pone una espera entre intentos."),
            new GuideTopic("Estructura de carpetas",
                "Prefabs/Gameplay: Interactables, Items, Barricade y Systems. Prefabs/UI: BreathingCanvas.\n" +
                "ScriptableObjects: Flags, Dialogues, Items, Noise, HallucinationProfiles.\n" +
                "Scripts por sistema: Interaction, Objects, Items, Barricade, Story, Dialogue, Sequences, Enemy, Hallucination, Player.")
        };

        public static readonly GuideTopic[] Recipes =
        {
            new GuideTopic("Puerta que se abre solo con una flag",
                "1. Agrega el prefab Door.\n2. En Required Flags agrega la flag necesaria.\n3. Opcional: asigna Locked Dialogue con lo que dice el personaje.\n4. En el objeto que debe dar la flag, ponla en Flags To Set On Interact."),
            new GuideTopic("Dialogo al entrar a una zona",
                "1. Agrega StoryTrigger_Zone y escalalo para cubrir la zona.\n2. Crea el dialogo en la pestana Historia.\n3. Asignalo a Dialogue On Trigger.\n4. Deja Trigger On Zone Enter activo. Fire Once evita que se repita."),
            new GuideTopic("Algo ocurre cuando cambia una flag",
                "1. Agrega StoryTrigger_Zone.\n2. Desactiva Trigger On Zone Enter y activa Trigger On Flag Changed.\n3. En Required Flags pon la flag a vigilar.\n4. Asigna un dialogo y/o llena On Triggered (por ejemplo encender una luz)."),
            new GuideTopic("Objeto que cambia el mundo al usarlo",
                "1. Agrega un Interactable_Basic.\n2. En On Interact pulsa +, arrastra el objeto a afectar y elige GameObject > SetActive (con el check activo para encender).\n3. Para un dialogo asigna Interact Dialogue."),
            new GuideTopic("Eleccion de final exclusiva",
                "1. Pon dos Examinable.\n2. Cada uno pone una flag distinta (FinalBueno o FinalMalo).\n3. Crea un objeto vacio con ExclusiveChoiceGroup y arrastra ambos a Options.\n4. Al usar uno, el otro se desactiva."),
            new GuideTopic("Esconder un objeto en un cajon",
                "1. Agrega Drawer.\n2. Pon el objeto en la escena donde debe aparecer (por ejemplo encima del mueble).\n3. Arrastralo a Contents. Queda oculto hasta abrir el cajon."),
            new GuideTopic("Secuencia de eventos con retraso",
                "1. Crea un objeto vacio con TimedEventSequence.\n2. Agrega pasos: cada uno con su retraso y su UnityEvent.\n3. Desde un interactuable, en On Interact llama a TimedEventSequence.Play."),
            new GuideTopic("Barricadear una puerta",
                "1. Agrega Door_Barricade.\n2. Agrega uno o mas Chair_Breakable cerca.\n3. El jugador rompe la silla, agarra una tabla y la coloca en la puerta.\n4. Para reaccionar al terminar usa la flag PuertaBarricadaLista o el evento On Completed del BarricadePoint."),
            new GuideTopic("Cambiar la alucinacion desde la historia",
                "En On Interact o On Triggered arrastra el HallucinationManager y elige Increase, Decrease o SetLevel. Para lanzar el minijuego elige BreathingMinigame.Begin."),
            new GuideTopic("Probar la historia sin jugarla entera",
                "En Play abre la pestana Historia: activa y desactiva flags con los toggles y reproduce dialogos con el boton Reproducir. Tambien puedes poner flags en Initial Flags del StoryManager para empezar con ellas activas.")
        };
    }
}
