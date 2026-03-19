using HighlightPlus;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace __Scripts
{
    [RequireComponent(typeof(Collider))]
    public class Terminal : MonoBehaviour
    {
        [Header("Connections (auto-managed at runtime)")]
        [SerializeField] private List<Terminal> connectedTerminals = new List<Terminal>();

        [Header("State")]
        [SerializeField] private bool isThereWater;
        [SerializeField] private bool emitter;      // set true on source terminals
        [SerializeField] private bool isEndTerminal; // for debugging

        [Header("Visuals")]
        [Tooltip("Particle prefab to spawn on open ends with water.")]
        [SerializeField] private ParticleSystem endWaterFxPrefab;
        [SerializeField] private HighlightEffect highlightEffect;

        private ParticleSystem _spawnedFx;

        public bool IsThereWater => isThereWater;
        public bool IsEmitter => emitter;

        private static readonly HashSet<Terminal> AllTerminals = new HashSet<Terminal>();

        private void OnEnable()
        {
            AllTerminals.Add(this);
            RecalculateNetwork();
        }

        private void OnDisable()
        {
            AllTerminals.Remove(this);
            RecalculateNetwork();
            DestroyFx();
        }

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        // ---------- Trigger connections ----------

        private void OnTriggerEnter(Collider other)
        {
            var otherTerminal = other.GetComponent<Terminal>();
            if (otherTerminal == null || otherTerminal == this) return;

            if (!connectedTerminals.Contains(otherTerminal))
                connectedTerminals.Add(otherTerminal);
            if (!otherTerminal.connectedTerminals.Contains(this))
                otherTerminal.connectedTerminals.Add(this);

            RecalculateNetwork();
        }

        private void OnTriggerExit(Collider other)
        {
            var otherTerminal = other.GetComponent<Terminal>();
            if (otherTerminal == null || otherTerminal == this) return;

            connectedTerminals.Remove(otherTerminal);
            otherTerminal.connectedTerminals.Remove(this);

            RecalculateNetwork();
        }

        // ---------- Graph / DFS ----------

        public bool Dfs(HashSet<Terminal> visited = null)
        {
            visited ??= new HashSet<Terminal>();
            if (!visited.Add(this)) return false;

            if (emitter) return true;

            foreach (var t in connectedTerminals)
            {
                if (t != null && t.Dfs(visited))
                    return true;
            }

            return false;
        }

        private static void RecalculateNetwork()
        {
            foreach (var terminal in AllTerminals)
            {
                bool newState = terminal.emitter || terminal.Dfs();
                if (terminal.isThereWater != newState)
                {
                    terminal.isThereWater = newState;
                }

                // update visuals for each terminal
                terminal.UpdateVisuals();
            }
        }

        // ---------- End detection + FX ----------

        /// <summary>
        /// Open end = has no connected terminals (free outlet).
        /// Adjust this logic if you want a different "end" definition.
        /// </summary>
        private bool IsEndTerminal()
        {
            int count = 0;
            foreach (var t in connectedTerminals)
            {
                if (t != null) count++;
            }

            isEndTerminal = count == 1 && !emitter;
            if (isEndTerminal)
            {
                
                Debug.Log($"Terminal '{name}' is an end terminal.");
            }
            else
            {
                Debug.Log($"Terminal '{name}' is NOT an end terminal (connected count: {count}, emitter: {emitter}).");
            }
            return isEndTerminal; // don't treat emitter as an "end" (optional)
        }

        private void UpdateVisuals()
        {
            bool shouldPlayFx = isThereWater && IsEndTerminal();

            if (shouldPlayFx)
            {
                if (_spawnedFx == null && endWaterFxPrefab != null)
                {
                    Debug.Log($"Spawning end water FX on terminal '{name}'.");
                    _spawnedFx = Instantiate(
                        endWaterFxPrefab,
                        transform.position,
                        transform.rotation,
                        transform  // parent so it follows the terminal
                    );
                }
                else if (_spawnedFx != null && !_spawnedFx.isPlaying)
                {
                    Debug.Log($"Playing existing end water FX on terminal '{name}'.");
                    _spawnedFx.Play();
                }
            }
            else
            {
                Debug.Log($"Destroying end water FX on terminal '{name}' (shouldPlayFx: {shouldPlayFx}).");
                DestroyFx();
            }

            if (isThereWater)
            {
                highlightEffect.highlighted = true;
            }
            else
            {
                highlightEffect.highlighted = false;
            }
        }

        private void DestroyFx()
        {
            if (_spawnedFx != null)
            {
                _spawnedFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                Destroy(_spawnedFx.gameObject);
                _spawnedFx = null;
            }
        }
    }
}
