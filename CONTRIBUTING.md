
# CONTRIBUTING.md

# Contributing to Frapper

Gracias por tu interés en contribuir a **Frapper**.

Frapper es una herramienta orientada a equipos **.NET + Dapper** que necesitan una forma robusta de versionar esquemas de base de datos mediante snapshots, diff estructural y migraciones SQL explícitas.

Este documento describe **cómo contribuir al proyecto**.

---

# Tipos de contribución

Las contribuciones pueden incluir:

- reportes de bugs
- mejoras en la CLI
- mejoras en el diff engine
- soporte para nuevos objetos de base de datos
- mejoras de documentación
- optimizaciones de performance
- nuevos tests

---

# Reportar bugs

Antes de crear un issue:

1. Verifica que el problema no haya sido reportado antes.
2. Incluye un ejemplo mínimo reproducible.

Idealmente incluye:

- versión de Frapper
- versión de .NET
- base de datos utilizada
- comandos ejecutados
- snapshots involucrados
- salida del error

Ejemplo de formato de bug report:

```
### Descripción

Al ejecutar `frapper migrate add` se genera un SQL incorrecto.

### Pasos para reproducir

1. Crear snapshot base
2. Modificar columna NVARCHAR(50) → NVARCHAR(100)
3. Ejecutar migrate add

### Resultado esperado

ALTER COLUMN NVARCHAR(100)

### Resultado actual

DROP + ADD COLUMN
```

---

# Proponer nuevas features

Las nuevas features deben:

- alinearse con la filosofía del proyecto
- mantener SQL explícito
- evitar dependencias innecesarias
- mantener el modelo snapshot-driven

Es recomendable abrir primero un **issue de discusión** antes de comenzar una implementación grande.

---

# Flujo de desarrollo

El flujo recomendado es:

1. Fork del repositorio
2. Crear una branch desde `main`
3. Implementar cambios
4. Agregar o actualizar tests
5. Crear Pull Request

Ejemplo:

```
git checkout -b feature/index-support
```

---

# Estándares de código

Frapper sigue convenciones comunes de C#.

## Naming

Clases:

```
PascalCase
```

Métodos:

```
PascalCase
```

Variables locales:

```
camelCase
```

Interfaces:

```
IInterfaceName
```

---

# Records del dominio

Los modelos de esquema utilizan `record` cuando es apropiado.

Ejemplo:

```
public sealed record DbTable(
    string Schema,
    string Name,
    IReadOnlyList<DbColumn> Columns
);
```

---

# Inmutabilidad

Siempre que sea posible:

- usar `record`
- usar `IReadOnlyList`
- evitar mutación del dominio

---

# Null safety

Evitar `null` innecesario.

Preferir:

- validaciones tempranas
- `ArgumentNullException`
- nullable reference types

---

# Testing

Todos los cambios significativos deben incluir tests.

Tipos de tests existentes:

```
Frapper.Core.Tests
Frapper.SqlServer.Tests
Frapper.EFMigrationEmitter.Tests
Frapper.Cli.Tests
```

---

# Reglas para tests

Un buen test debe:

- ser determinístico
- no depender de estado externo
- usar fakes cuando sea posible
- tener nombres claros

Ejemplo:

```
Diff_ShouldDetectAddedColumn
```

---

# Ejecutar tests

Para ejecutar todos los tests:

```
dotnet test
```

---

# Ejecutar build

```
dotnet build
```

---

# Ejecutar la CLI localmente

```
dotnet run --project src/Frapper.Cli
```

---

# Empaquetar como herramienta

```
dotnet pack src/Frapper.Cli/Frapper.Cli.csproj -c Release
```

---

# Instalar tool local o global

Ejemplo global:

```
dotnet tool install --global frapper --add-source ./nupkg
```

Actualizar tool:

```
dotnet tool update --global frapper --add-source ./nupkg
```

---

# Áreas donde se necesitan contribuciones

Actualmente las áreas más interesantes para contribuir son:

## Diff Engine

Mejoras posibles:

- rename detection
- diff semántico
- mejor detección de cambios destructivos

---

## Objetos de base de datos

Soporte para:

- indexes
- foreign keys
- unique constraints
- views
- triggers
- stored procedures

---

## CLI

Mejoras posibles:

- nuevos comandos
- mejor UX
- validaciones adicionales
- mensajes de error más claros

---

## Multi-database

Soporte futuro para:

- PostgreSQL
- MySQL
- SQLite

---

# Estilo de commits

Preferir commits claros.

Ejemplo:

```
feat(diff): add detection for column length changes
```

```
fix(cli): resolve null connection handling
```

```
docs(readme): update migration workflow
```

---

# Pull Requests

Un Pull Request ideal incluye:

- descripción clara
- contexto del cambio
- tests asociados
- impacto esperado

---

# Filosofía del proyecto

Frapper intenta mantener siempre tres principios:

1. SQL explícito
2. control del esquema
3. simplicidad operativa

Las contribuciones deberían respetar estos principios.

---

# Licencia

Al contribuir al proyecto aceptas que tu contribución será publicada bajo la licencia **MIT** del repositorio.
