﻿using MauiCleanTodos.Application.Common.Exceptions;
using MauiCleanTodos.Application.TodoItems.Commands.CreateTodoItem;
using MauiCleanTodos.Application.TodoItems.Commands.UpdateTodoItem;
using MauiCleanTodos.Application.TodoItems.Commands.UpdateTodoItemDetail;
using MauiCleanTodos.Application.TodoLists.Commands.CreateTodoList;
using MauiCleanTodos.Domain.Entities;
using MauiCleanTodos.Domain.Enums;
using FluentAssertions;
using NUnit.Framework;
using MauiCleanTodos.Shared.TodoItems;

namespace MauiCleanTodos.Application.IntegrationTests.TodoItems.Commands;

using static Testing;

public class UpdateTodoItemDetailTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidTodoItemId()
    {
        var command = new UpdateTodoItemCommand
        {
            Item = new TodoItemDto
            {
                Id = 99,
                Title = "New Title"
            }
        };
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldUpdateTodoItem()
    {
        var userId = await RunAsDefaultUserAsync();

        var listId = await SendAsync(new CreateTodoListCommand
        {
            Title = "New List"
        });

        var itemId = await SendAsync(new CreateTodoItemCommand
        {
            Item = new NewTodoItemDto
            {
                ListId = listId,
                Title = "New Item"
            }
        });

        var command = new UpdateTodoItemDetailCommand
        {
            Item = new TodoItemDto
            {
                Id = itemId,
                ListId = listId,
                Note = "This is the note.",
                Priority = (int)PriorityLevel.High
            }
        };

        await SendAsync(command);

        var item = await FindAsync<TodoItem>(itemId);

        item.Should().NotBeNull();
        item!.ListId.Should().Be(command.Item.ListId);
        item.Note.Should().Be(command.Item.Note);
        item.Priority.Should().Be((PriorityLevel)command.Item.Priority);
        item.LastModifiedBy.Should().NotBeNull();
        item.LastModifiedBy.Should().Be(userId);
        item.LastModified.Should().NotBeNull();
        item.LastModified.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMilliseconds(10000));
    }
}
