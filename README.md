# ColorMixer

Postprocesado del G-Code de Prusaslicer para la A20M de Geeetech

## Codigo Gcode

Este es el codigo que se usa para una Geeetech A20M con tobera situada en el lateral del motor del eje X.

```gcode
    {if layer_num >= 0}
    ; Cambio de herramienta #{layer_num}
    M211 S0                                                                                     ; Desactivamos los finales de carrera software para poder salirme fuera

    G92 E0                                                                                      ; Reseteamos la distancia absoluta del extrusor a 0
    G1 E-{retract_length[current_extruder]} F{retract_speed[current_extruder]*max_print_speed}  ; Retraemos el filamento en la herramienta antigua
    G1 X0 F4000                                                                                 ; Nos vamos a la posición 0
    G1 X-10 F8000                                                                               ; Entramos dentro de la tobera
    G1 E{retract_length[current_extruder]} F{deretract_speed[current_extruder]*max_print_speed} ; Extruimos lo que hemos retraido
    
    
    T[next_extruder]
    
    ;py$ {filament_colour[next_extruder]}
    
    M104 S[temperature_[next_extruder]]                                                         ; Prepare nozzle new temperature 
    ;M109 S[temperature_[next_extruder]]                                                        ; Esperamos a que se caliente el extrusor
    M117 Nº Layer {layer_num + 1}                                                               ; Mensaje de depuración
    
    G92 E0                                                                                      ; Reseteamos la distancia absoluta del extrusor a 0
    ;py$ G1 E50 F500                                                                            ; Extruimos el filamento
    G1 E-{retract_length[next_extruder]} F{retract_speed[next_extruder]*max_print_speed}        ; Retraemos el filamento en la herramienta antigua
    G1 X0 F8000                                                                                 ; Salimos de la tobera
    G1 E{retract_length[next_extruder]} F{deretract_speed[next_extruder]*max_print_speed}       ; Extruimos lo que hemos retraido
    G92 E0                                                                                      ; Reseteamos la distancia absoluta del extrusor a 0
    
    M211 S1                                                                                     ; Activo de nuevos limites
    ;Fin cambio de herramienta
    {endif}

```

Se usa los comodines `;py$` para denotar el codigo que la herramienta debe procesar. De momento solo procesa las lineas indicadas en el ejemplo:

- **`;py$ {filament_colour[next_extruder]}`** Indica el color del filamento, el cual el postprocesador cogerá para crear el codigo G-code necesario para crear el mezclado de colores.

- **`;py$ G1 E50 F500`** Se usa para indicar el filamento que se desea extruir para el cambio de color. Mandandosela al Postprocesador, consegimos, según la tonalidad del color, añadir un factor de multiplicación a la cantidad de extrusión. No se necesita la misma cantidad de extrusión para un filamento negro *(se usará el valor indicado)* que para un filamneto blannco *(se usará la cantidad indicada mas la mitad de ésta)*.

***
