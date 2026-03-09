
# Frapper Roadmap

Este documento describe la dirección de evolución planificada para **Frapper**.

Frapper nació como una herramienta para resolver un problema concreto:

equipos que usan **Dapper** necesitan una forma robusta de **versionar el esquema de base de datos** sin depender de un ORM.

El proyecto ya cuenta con una base funcional sólida, pero todavía tiene varias áreas donde puede crecer.

Este roadmap describe esa evolución.

---

# Estado actual

Frapper actualmente ya implementa:

- introspección de esquema de SQL Server
- snapshots determinísticos del esquema
- comparación estructural entre snapshots
- generación de migraciones SQL
- aplicación de migraciones con historial
- detección de drift entre snapshot y base de datos
- CLI funcional (`init`, `snapshot`, `diff`, `migrate add`, `migrate apply`)
- suite de tests automatizados

Esto convierte a Frapper en un **prototipo funcional real**, capaz de manejar migraciones simples en entornos controlados.

---

# Visión

La visión a largo plazo de Frapper es convertirse en una herramienta que permita:

- migraciones seguras
- SQL explícito
- control database‑first
- integración natural con Dapper

sin obligar a adoptar un ORM completo.

En términos de posicionamiento, Frapper busca situarse conceptualmente entre:

| Herramienta | Enfoque |
|---|---|
| EF Core Migrations | ORM‑first |
| Flyway | SQL‑first |
| Liquibase | Declarative‑first |
| Frapper | Snapshot‑driven + SQL explícito |

---

# Fase 1 — Base sólida (actual)

Estado: **implementado o muy cercano**

Características:

- snapshot generation
- snapshot diff
- migration SQL emission
- migration execution
- migration history table
- drift detection
- CLI funcional

Objetivo de esta fase:

Validar que el modelo **snapshot → diff → migration** funciona correctamente.

---

# Fase 2 — Soporte de más objetos de base de datos

Objetivo: ampliar el modelo de esquema.

Actualmente Frapper soporta principalmente:

- tablas
- columnas
- primary keys

Próximos objetos a soportar:

## Índices

- index detection
- index diff
- index creation
- index drop

## Foreign Keys

- FK introspection
- FK diff
- FK creation
- FK removal

## Unique constraints

- detección
- diff
- migraciones

---

# Fase 3 — Mejoras del Diff Engine

El motor de diff puede volverse más inteligente.

## Rename detection

Evitar:

DROP COLUMN  
ADD COLUMN

cuando en realidad hubo:

RENAME COLUMN

Esto requiere heurísticas basadas en:

- tipo
- posición
- similitud de nombre

---

## Diff más semántico

Detectar cambios más complejos:

- default constraint changes
- identity changes
- nullable transitions
- length changes

---

# Fase 4 — Mejoras del sistema de migraciones

Actualmente las migraciones son SQL simples.

Posibles mejoras:

## Down migrations más robustas

Hoy las migraciones se centran en el camino forward.

Se puede mejorar:

- generación automática de DOWN
- rollback controlado

---

## Validaciones de seguridad

Ejemplos:

- bloquear drop column en producción
- advertir sobre data loss
- detectar cambios destructivos

---

## Dry‑run mode

Permitir ejecutar:

frapper migrate apply --dry-run

para ver qué ocurriría sin modificar la base.

---

# Fase 5 — Mejoras en la CLI

La CLI actual es funcional pero minimalista.

Posibles mejoras:

## UX de comandos

- mensajes más claros
- mejores errores
- progreso visible

---

## Nuevos comandos

Ejemplos posibles:

frapper status  
frapper history  
frapper validate

---

## Configuración más flexible

Mejor soporte para:

frapper.config.json

incluyendo:

- múltiples conexiones
- múltiples entornos

---

# Fase 6 — Modelo híbrido (snapshot + code)

Una posible evolución interesante es generar snapshots desde **modelos C# opcionales**.

Ejemplo conceptual:

Order.cs  
User.cs

↓

schema.snapshot.json

Esto permitiría un modelo híbrido:

- database‑first
- pero opcionalmente model‑driven

---

# Fase 7 — Soporte multi‑database

Actualmente Frapper es **SQL Server‑only**.

La arquitectura ya permite expandirse a:

PostgreSQL  
MySQL  
SQLite

Esto requeriría:

- nuevos SchemaReader
- normalización de tipos
- SQL emitters específicos

---

# Fase 8 — Integración CI/CD

Frapper puede integrarse fácilmente en pipelines.

Ejemplos:

## Validación en CI

frapper diff --connection Production

Detectar drift.

## Migraciones automáticas

frapper migrate apply

durante deploy.

---

# Fase 9 — Observabilidad y auditoría

Mejorar trazabilidad del sistema.

Ejemplos:

- logs estructurados
- historial enriquecido de migraciones
- metadata de migraciones

---

# Fase 10 — Ecosistema

A largo plazo Frapper podría incluir:

## Visualización de esquema

Generación de diagramas:

frapper diagram

---

## Exportación de documentación

frapper docs

Generar documentación automática del esquema.

---

# Prioridades inmediatas

Las siguientes mejoras tienen mayor impacto inmediato:

1. soporte para índices
2. soporte para foreign keys
3. rename detection
4. validaciones de migraciones destructivas
5. mejoras de CLI

---

# Contribuciones

Las contribuciones son bienvenidas.

Especialmente en áreas como:

- diff engine
- soporte de objetos de base de datos
- testing
- experiencia de CLI

---

# Resumen

Frapper ya demuestra que el modelo:

snapshot → diff → migration → apply

es viable para equipos **Dapper‑first**.

El roadmap busca llevar esa base a una herramienta más completa manteniendo tres principios:

- SQL explícito
- control del esquema
- simplicidad operativa
