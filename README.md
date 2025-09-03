# 🎛️ Wave Engine Core - Sistema de Microondas

Sistema de controle de microondas desenvolvido em **ASP.NET Core 9.0** com **Clean Architecture**.

## 🚀 Como Executar

```bash
# Clone o repositório
git clone <repository-url>
cd wave-engine-core

# Execute a aplicação
dotnet run --project WaveEngineCore

# Acesse no navegador
http://localhost:5296
```

## ✨ Funcionalidades

### Nível 1 - Básico

- ✅ Aquecimento manual (1-120s, potência 1-10)
- ✅ Início rápido (30s, potência 10)
- ✅ Pausar/Retomar aquecimento
- ✅ Adicionar tempo (+30s)
- ✅ Visualização de progresso

### Nível 2 - Programas Pré-definidos

- ✅ **Pipoca** (180s, P7)
- ✅ **Leite** (300s, P5)
- ✅ **Carnes de boi** (840s, P4)
- ✅ **Frango** (480s, P7)
- ✅ **Feijão** (480s, P9)

## 🏗️ Estrutura do Projeto

```
wave-engine-core/
├── Microwave.Domain/          # Regras de negócio e entidades
│   ├── MicrowaveOven.cs
│   ├── DTOs/
│   └── Validators/
├── Microwave.Application/     # Serviços da aplicação
│   └── MicrowaveService.cs
├── Microwave.Infrastructure/  # Persistência e infraestrutura
├── Microwave.Tests/          # Testes (94 passando)
│   ├── Unit/
│   ├── Integration/
│   └── Scenarios/
├── WaveEngineCore/           # Interface web (Razor Pages)
│   ├── Pages/Index.cshtml
│   ├── Controllers/
│   └── wwwroot/
└── WaveEngineCore.sln
```

## 🧪 Testes

```bash
# Executar todos os testes
dotnet test

# Resultado esperado: 94 testes passando
```

## �️ Tecnologias

- **Backend**: ASP.NET Core 9.0, C# 12
- **Frontend**: Razor Pages, HTML5, CSS3, JavaScript
- **Arquitetura**: Clean Architecture, Strategy Pattern, Factory Pattern
- **Testes**: xUnit (94 testes)
- **Persistência**: Session Storage

## 📝 Padrões Implementados

- **Clean Architecture** (Domain, Application, Infrastructure, Presentation)
- **Strategy Pattern** (Validação de tempo)
- **Factory Pattern** (Criação de validadores)
- **Repository Pattern** (IStateStorage)
- **Dependency Injection**

---

**Status**: ✅ Funcional | 🧪 94 testes passando | 🏗️ Clean Architecture
