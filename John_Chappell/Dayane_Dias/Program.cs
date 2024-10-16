// Corrigido pelo professor 16/10/2024 10:36
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

using Dayane_Dias.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDataContext>();

var app = builder.Build();

app.MapGet("/api", () => "Avaliação - A2");

// Cadastrar Funcionário
app.MapPost("/api/funcionario/cadastrar", async ([FromBody] Funcionario funcionario, AppDataContext ctx) => {

    ctx.Add(funcionario);
    await ctx.SaveChangesAsync();

    return Results.Created("", funcionario);
});

// Listar Funcionários
app.MapGet("/api/funcionario/listar", (AppDataContext ctx) => {
    if (ctx.Funcionarios.Any())
        return Results.Ok(ctx.Funcionarios.ToList());

    return Results.NotFound("Nenhum funcionário cadastrado!");
});

// Cadastrar Folha
app.MapPost("/api/folha/cadastrar", async ([FromBody] Folha folha, AppDataContext ctx) => {

    Funcionario? funcionario = ctx.Funcionarios.Find(folha.FuncionarioId);

    if (funcionario == null)
        return Results.NotFound("Funcionário não encontrado!");
    
    folha.Funcionario = funcionario;

    float salario_bruto = folha.Quantidade * folha.Valor / folha.Mes;
    float salario_liquido = salario_bruto;
    float imposto_renda = 0.0f;
    float inss = 0.0f;

    // Imposto de Renda
    if (salario_bruto <= 1_903.98 && salario_bruto >= 2_826.65)
        imposto_renda = (float) (salario_bruto * 0.075);
    
    if (salario_bruto >= 2_826.66 && salario_bruto <= 3_751.05)
        imposto_renda = (float) (salario_bruto * 0.15);

    if (salario_bruto >= 3_751.06 && salario_bruto <= 4_664.68)
        imposto_renda = (float) (salario_bruto * 0.225);
    
    if (salario_bruto > 4_664.68)
        imposto_renda = (float) (salario_bruto * 0.275);

    // INSS
    if (salario_bruto <= 1_693.72)
        inss = (float) (salario_bruto * 0.08);

    if (salario_bruto >= 1_693.73 && salario_bruto <= 2_822.90)
        inss = (float) (salario_bruto * 0.09);

    if (salario_bruto >= 2_822.91 && salario_bruto <= 5_645.80)
        inss = (float) (salario_bruto * 0.11);

    if (salario_bruto > 5_645.81)
        inss = 621.03f;

    // FGTS
    float fgts = (float) (salario_bruto * 0.08);


    folha.SalarioBruto = salario_bruto;
    salario_liquido = salario_bruto - imposto_renda - inss;

    folha.ImpostoIrrf = imposto_renda;
    folha.ImpostoInss = inss;
    folha.ImpostoFgts = fgts;
    folha.SalarioLiquido = salario_liquido;

    ctx.Add(folha);
    await ctx.SaveChangesAsync();

    return Results.Created("", folha);
});

// Listar Folhas
app.MapGet("/api/folha/listar", (AppDataContext ctx) => {
    if (ctx.Funcionarios.Any())
        return Results.Ok(ctx.Folhas.ToList());

    return Results.NotFound("Nenhuma folha cadastrada!");
});

// Buscar Folha
app.MapGet("/api/folha/buscar/{cpf}/{mes}/{ano}", ([FromRoute] string cpf, [FromRoute] int mes, [FromRoute] int ano,
AppDataContext ctx) => {
    Funcionario? funcionario = ctx.Funcionarios.FirstOrDefault(f => f.Cpf == cpf); 

    Folha? folha = ctx.Folhas.Include(f => f.Funcionario).FirstOrDefault(f => f.Funcionario != null && f.Funcionario.Cpf == cpf && f.Mes == mes && f.Ano == ano);

    if (folha != null)
        return Results.Ok(folha);

    return Results.NotFound("Nenhuma folha cadastrada!");
});

app.Run();