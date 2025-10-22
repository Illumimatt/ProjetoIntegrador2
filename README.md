<h3 align="center">projeto.integrador.ii</h3>
<p align="center"> Repositório dedicado ao desenvolvimento de um jogo eletrônico completo, do conceito ao lançamento </p>

<p align="center">
  <img src="https://img.shields.io/badge/status-Em%20Planejamento-yellow" alt="Status do Projeto: Em Planejamento">
  <img src="https://img.shields.io/badge/engine-Unity-black?logo=unity" alt="Game Engine: Unity">
  <!-- <img src="https://img.shields.io/badge/licen%C3%A7a-MIT-blue" alt="Licença: MIT"> -->
</p>

## O que estamos fazendo?

O objetivo é desenvolver um jogo eletrônico completo e polido, desde sua concepção inicial de design até uma versão final pronta para ser lançada em plataformas digitais. O projeto abrange todas as fases do desenvolvimento, incluindo:

- Game Design e Lógica de Jogo.
- Programação de Lógica, Ferramentas e Interface (UI/UX).
- Criação de arte visual, modelos 3D e design de áudio.
- Testes de qualidade e validação.

## Por que este projeto é importante?

Este projeto representa a ponte entre o conhecimento acadêmico e a experiência prática do mercado. Nossa motivação é criar um produto real e tangível, superando os requisitos da disciplina para construir um portfólio poderoso. Os pilares da nossa motivação são:

- **Aprender:** Aplicar teorias em um desafio real, dominando o ciclo de vida completo de um produto de software.
- **Nos Desafiar:** Superar nossos limites técnicos e criativos, desenvolvendo habilidades essenciais para a indústria de jogos.
- **Lançar:** A maior motivação é transformar o projeto em nosso primeiro jogo lançado, uma prova concreta de nossa capacidade de entrega.

## Quem está envolvido e é responsável pelo sucesso do projeto?

A equipe é formada por um grupo com habilidades complementares, cada um responsável por uma área crucial do desenvolvimento:

| Nome                                                  | Funções                                          |
| :---------------------------------------------------- | :----------------------------------------------- |
| [**Bruno d'Luka**](https://www.github.com/bdlukaa)    | Programador de Lógica de Jogo e Ferramentas      |
| **Caroline Machado**                                  | Artista Principal e Designer de Áudio            |
| **Julia Costa**                                       | Programadora de Interface (UI/UX) e Modelista 3D |
| [**Matheus Kollmann**](https://github.com/Illumimatt) | Game Designer e Gerente de Projeto               |

## Onde o projeto será desenvolvido e acompanhado?

- **Gestão e Código-fonte:** [`GitHub`](https://github.com/Illumimatt/ProjetoIntegrador2)
- **Game Engine:** Unity
- **Ferramentas de Desenvolvimento:** Visual Studio Code (VSCode)
- **Arte e Modelagem 3D:** Blender e Nomad Sculpt
- **Planejamento e tarefas:** [`Google Drive`](https://drive.google.com/drive/folders/1RwOFW-68JM9Si7ZjIWvBx5EgL9dN5c1B?usp=sharing)
- **Link do PMCanvas:** [`Miro`](https://miro.com/app/board/uXjVJUXGZKc=/?share_link_id=260673801956)

## 🏛️ Arquitetura da Aplicação

A arquitetura do "Dekora" segue os padrões de design de software mais comuns para o desenvolvimento de jogos na Unity, primariamente a **Arquitetura Baseada em Componentes** e o **Padrão de Gerenciadores (Singleton)**.

<p align="center">
  <img src="arquitetura/DiagramaDeClassesDekora.png" alt="Diagrama de Classes da Arquitetura do Dekora" width="800"/>
  <br/>
  <sup>Diagrama de Classes da arquitetura do sistema.</sup>
</p>

A estrutura se divide nas seguintes áreas de responsabilidade:

### 1. Camada de Gerenciamento (Managers)
Esta é a "espinha dorsal" do sistema. É composta por classes de alto nível que gerenciam o estado do jogo e os sistemas centrais. Utilizamos o padrão Singleton para garantir que exista apenas uma instância dos gerenciadores globais, facilitando o acesso a partir de qualquer ponto do código.

* **`GameManager` (Singleton):** Orquestra o fluxo principal do jogo. Controla o estado (MainMenu, Playing, Paused) e coordena os outros gerenciadores.
* **`LevelManager`:** Responsável por carregar, descarregar e reiniciar as fases (Level).
* **`SaveManager`:** Abstrai toda a lógica de salvar e carregar o progresso.
* **`AudioManager` (Singleton):** Gerencia a reprodução de música e efeitos sonoros.

### 2. Camada de Interface (UI)
Esta camada é responsável por toda a interação do usuário com os menus e elementos de interface. Ela segue um padrão de Herança para reutilização de código.

* **`UIManager`:** Gerencia quais telas estão ativas.
* **`UIScreen` (Abstrata):** Uma classe base que define o comportamento padrão de uma tela (Mostrar, Esconder).
* **`MainMenuScreen`, `SettingsScreen`, etc.:** Implementações concretas que herdam de `UIScreen` e cuidam de botões e sliders específicos.

### 3. Camada de Lógica de Jogo (Gameplay Core)
Esta camada contém os objetos e sistemas com os quais o jogador interage diretamente. Este é o coração da Arquitetura Baseada em Componentes da Unity.

* **`Level`:** Contém os dados da fase, os objetos e o sistema de grid.
* **`DecorativeObject`:** Um componente (script) que será anexado a um objeto 3D no jogo. Ele contém a lógica para ser selecionado, arrastado e solto.
* **`GridSystem`:** Uma classe de lógica auxiliar que fornece a funcionalidade de "snap" ao grid.

## Quando serão feitas as entregas principais?

- **Status atual:** Em Planejamento
- **Planejamento:** até 18/08/2025
- **Protótipos:** até 15/10/2025
- **Testes:** até 22/11/2025
- **Entrega Final (Relatório):** até 01/12/2025

---

<div align="center">
    <p>Feito com ❤️ pela equipe do Projeto Integrador</p>
</div>
