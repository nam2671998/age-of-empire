using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Helper component to handle command input for selected units
/// This can be attached to a manager object to handle right-click commands
/// </summary>
public class InputCommandResolverController : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private KeyCode commandKey = KeyCode.Mouse1;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask unitLayer;
    [SerializeField] private LayerMask resourceLayer;
    [SerializeField] private LayerMask buildingLayer;
    
    [Header("References")]
    [SerializeField] private BoxSelection boxSelection;
    [SerializeField] private Camera mainCamera;

    private List<ICommandResolver> resolvers;
    
    void Awake()
    {
        if (boxSelection == null)
        {
            boxSelection = FindObjectOfType<BoxSelection>();
        }
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        resolvers = new List<ICommandResolver>
        {
            new AttackCommandResolver(unitLayer),
            new HarvestCommandResolver(resourceLayer),
            new BuildCommandResolver(buildingLayer),
            new MoveCommandResolver(groundLayer),
        };
    }
    
    void Update()
    {
        if (Input.GetKeyDown(commandKey))
        {
            HandleInput();
        }
    }
    
    private void HandleInput()
    {
        if (mainCamera == null || boxSelection == null || resolvers == null || resolvers.Count == 0)
            return;

        if (!Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out var hit, Mathf.Infinity))
            return;

        foreach (var resolver in resolvers)
        {
            if (resolver == null || !resolver.CanResolve(hit))
                continue;

            IssueCommand(resolver, hit);
            return;
        }
    }
    
    private void IssueCommand(ICommandResolver resolver, RaycastHit hit)
    {
        var selected = ListPool<IGameSelectable>.Get();
        boxSelection.GetSelectedObjects(selected);

        foreach (var selectable in selected)
        {
            if (!selectable.GetGameObject().TryGetComponent(out CommandExecutor commandExecutor))
                continue;

            ICommand command = resolver.CreateCommand(hit);
            if (command != null)
                commandExecutor.SetCommand(command);
        }

        ListPool<IGameSelectable>.Release(selected);
    }
}

