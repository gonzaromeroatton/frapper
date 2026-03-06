# Roadmap

Este roadmap describe una evolución razonable para Frapper desde su estado actual hacia una herramienta de migraciones más completa para equipos .NET que trabajan con Dapper y SQL Server.

---

## Guiding principles

Antes de agregar features, Frapper debería mantener estas prioridades:

1. **Determinism first**
2. **Explicit SQL over magic**
3. **Database-first modeling**
4. **Safety for destructive changes**
5. **Good test coverage before broader feature support**

---

## Current baseline

Hoy Frapper ya cuenta con:

- lectura de esquema SQL Server
- modelo de dominio del esquema
- normalización de tipos
- snapshots determinísticos
- diff estructural básico
- emisión de SQL de migración
- warnings para ciertos cambios sensibles
- tests relevantes

---

## Phase 1 — Hardening the current foundation

Objetivo: reforzar lo que ya existe antes de expandir demasiado el alcance.

### Goals

- mejorar cobertura de tests
- reforzar determinismo del snapshot
- estabilizar output del emitter
- revisar naming y contracts internos
- agregar documentación técnica y ejemplos

### Deliverables

- tests adicionales para edge cases
- fixtures de esquema más realistas
- validación más estricta del SQL emitido
- documentación de invariantes del diff engine

---

## Phase 2 — Better relational modeling

Objetivo: mejorar la representación del esquema relacional real.

### Priorities

- foreign keys
- unique constraints
- default constraints más ricos
- nullability changes más robustos
- precision / scale / length changes mejor modelados

### Benefits

Esto permitiría que Frapper describa mejor cambios reales de negocio y reduzca intervención manual.

---

## Phase 3 — Index support

Objetivo: agregar soporte de índices.

### Scope

- lectura de índices
- snapshot de índices
- diff de índices
- SQL para create/drop index
- warnings para cambios costosos

### Why it matters

Muchos cambios de performance en producción pasan por índices.  
Sin este soporte, el versionado del esquema queda incompleto.

---

## Phase 4 — Smarter change detection

Objetivo: evitar que algunos cambios válidos aparezcan como operaciones demasiado primitivas.

### Main target

- rename detection para tablas
- rename detection para columnas

### Challenge

Detectar rename de forma confiable no es trivial.  
Hay que evitar falsos positivos que conviertan un drop/create real en un rename incorrecto.

### Suggested approach

- heurísticas conservadoras
- feature flags
- warnings cuando exista ambigüedad
- opción para override manual

---

## Phase 5 — Safer migrations

Objetivo: mejorar seguridad operativa.

### Ideas

- warnings para operaciones destructivas
- clasificación de riesgo de migración
- detección de posibles data-loss scenarios
- modo dry-run
- salida de reportes de impacto

### Example

- drop column → warning alto
- alter nullability → warning medio/alto
- default change → warning visible
- large table operations → warning de performance

---

## Phase 6 — Stronger CLI

Objetivo: exponer mejor el motor interno.

### Candidate commands

```bash
frapper snapshot
frapper diff
frapper migrate add <Name>
frapper migrate script
frapper validate
frapper inspect
```

### Nice additions

- `--output`
- `--format`
- `--dry-run`
- `--verbose`
- `--fail-on-warning`

### Benefit

Una buena CLI convierte a Frapper desde una buena librería interna en una herramienta usable de verdad.

---

## Phase 7 — Snapshot serialization and storage

Objetivo: formalizar almacenamiento y lectura de snapshots.

### Possibilities

- JSON determinístico
- metadata mínima de versión
- checksum / hash de consistencia
- compatibilidad entre versiones del snapshot format

### Why this matters

El snapshot será un artefacto central para Git, CI/CD y auditoría de cambios.

---

## Phase 8 — Broader database object support

Objetivo: ampliar cobertura del catálogo.

### Candidates

- views
- stored procedures
- functions
- triggers
- schemas
- check constraints

### Trade-off

No todos estos objetos deben entrar al mismo tiempo.  
Conviene priorizar primero los que agreguen más valor con menor ambigüedad.

---

## Phase 9 — Multi-database architecture

Objetivo: preparar Frapper para soportar otros motores a futuro.

### Potential targets

- PostgreSQL
- MySQL

### Requirements

- abstraer reader por provider
- abstraer normalización de tipos
- abstraer emitter por dialecto
- mantener el core independiente del motor

### Recommendation

No comenzar aquí demasiado pronto.  
Primero conviene hacer muy bien SQL Server.

---

## Phase 10 — Production readiness

Objetivo: acercar Frapper a uso real en pipelines de equipos.

### Desirable capabilities

- output estable en CI
- exit codes útiles
- integración con pipelines
- documentación de operación
- versión empaquetable/distribuible
- estrategia clara de compatibilidad

---

## Prioritized backlog proposal

### Highest priority

1. hardening + tests
2. richer constraints
3. index support
4. stronger CLI
5. migration safety warnings

### Medium priority

6. snapshot serialization
7. rename detection
8. impact/risk analysis

### Longer-term priority

9. programmable objects
10. multi-database support

---

## Non-goals for now

Para no dispersar el proyecto demasiado pronto, probablemente **no** conviene priorizar todavía:

- diseñador gráfico de migraciones
- UI web
- auto-fix “magic mode”
- rename detection agresivo sin garantías
- soporte multi-DB antes de estabilizar SQL Server

---

## Definition of success

Frapper será realmente convincente cuando pueda ofrecer, de forma confiable:

- snapshots determinísticos
- diff entendible y auditable
- SQL explícito
- warnings útiles
- CLI usable
- buen soporte para los objetos más importantes del esquema SQL Server

---

## Summary

La evolución natural del proyecto no es “agregar todo”, sino avanzar en este orden:

**hacerlo correcto → hacerlo seguro → hacerlo usable → hacerlo más amplio**

Ese orden encaja muy bien con la identidad técnica de Frapper.
