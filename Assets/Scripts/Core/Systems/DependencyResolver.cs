using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GatchaSpire.Core.Systems
{
    /// <summary>
    /// システム間の依存関係を解決
    /// </summary>
    public class DependencyResolver
    {
        private readonly Dictionary<string, IUnityGameSystem> systems;
        private readonly Dictionary<string, List<string>> dependencies;
        private readonly Dictionary<string, SystemPriority> priorities;

        public DependencyResolver()
        {
            systems = new Dictionary<string, IUnityGameSystem>();
            dependencies = new Dictionary<string, List<string>>();
            priorities = new Dictionary<string, SystemPriority>();
        }

        /// <summary>
        /// システムを登録
        /// </summary>
        public void RegisterSystem(IUnityGameSystem system, string[] dependsOn = null)
        {
            var systemName = system.GetSystemName();
            systems[systemName] = system;
            priorities[systemName] = system.Priority;
            
            if (dependsOn != null && dependsOn.Length > 0)
            {
                dependencies[systemName] = new List<string>(dependsOn);
            }
        }

        /// <summary>
        /// 依存関係に基づいた初期化順序を解決
        /// </summary>
        public List<IUnityGameSystem> ResolveInitializationOrder()
        {
            var resolved = new List<string>();
            var visiting = new HashSet<string>();
            var result = new List<IUnityGameSystem>();

            // 全システムに対してトポロジカルソート
            foreach (var systemName in systems.Keys)
            {
                if (!resolved.Contains(systemName))
                {
                    Visit(systemName, resolved, visiting, result);
                }
            }

            return result;
        }

        /// <summary>
        /// トポロジカルソートの再帰実装
        /// </summary>
        private void Visit(string systemName, List<string> resolved, HashSet<string> visiting, List<IUnityGameSystem> result)
        {
            if (visiting.Contains(systemName))
            {
                throw new InvalidOperationException($"循環依存が検出されました: {systemName}");
            }

            if (resolved.Contains(systemName))
            {
                return;
            }

            visiting.Add(systemName);

            // 依存関係を先に解決
            if (dependencies.TryGetValue(systemName, out var deps))
            {
                foreach (var dependency in deps)
                {
                    if (!systems.ContainsKey(dependency))
                    {
                        throw new InvalidOperationException($"依存システム '{dependency}' が見つかりません (要求元: {systemName})");
                    }
                    Visit(dependency, resolved, visiting, result);
                }
            }

            visiting.Remove(systemName);
            resolved.Add(systemName);
            
            if (systems.TryGetValue(systemName, out var system))
            {
                result.Add(system);
            }
        }

        /// <summary>
        /// 依存関係の検証
        /// </summary>
        public ValidationResult ValidateDependencies()
        {
            var result = new ValidationResult();

            try
            {
                // 循環依存チェック
                ResolveInitializationOrder();
                
                // 存在しない依存関係チェック
                foreach (var (systemName, deps) in dependencies)
                {
                    foreach (var dependency in deps)
                    {
                        if (!systems.ContainsKey(dependency))
                        {
                            result.AddError($"システム '{systemName}' が存在しない依存関係 '{dependency}' を参照しています");
                        }
                    }
                }

                // 優先度と依存関係の整合性チェック
                foreach (var (systemName, deps) in dependencies)
                {
                    var systemPriority = priorities[systemName];
                    foreach (var dependency in deps)
                    {
                        if (priorities.TryGetValue(dependency, out var depPriority))
                        {
                            if ((int)systemPriority <= (int)depPriority)
                            {
                                result.AddWarning($"システム '{systemName}' の優先度が依存関係 '{dependency}' より高いか同等です");
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException e)
            {
                result.AddError(e.Message);
            }

            return result;
        }

        /// <summary>
        /// システムの依存関係グラフを取得
        /// </summary>
        public Dictionary<string, List<string>> GetDependencyGraph()
        {
            return new Dictionary<string, List<string>>(dependencies);
        }

        /// <summary>
        /// 依存関係情報を文字列で取得
        /// </summary>
        public string GetDependencyInfo()
        {
            var info = "=== システム依存関係 ===\n";
            
            foreach (var systemName in systems.Keys.OrderBy(s => priorities[s]))
            {
                info += $"{systemName} (優先度: {priorities[systemName]})\n";
                
                if (dependencies.TryGetValue(systemName, out var deps) && deps.Count > 0)
                {
                    info += $"  依存: {string.Join(", ", deps)}\n";
                }
                else
                {
                    info += "  依存: なし\n";
                }
            }

            return info;
        }

        /// <summary>
        /// 特定システムの依存関係を取得
        /// </summary>
        public List<string> GetSystemDependencies(string systemName)
        {
            return dependencies.TryGetValue(systemName, out var deps) ? new List<string>(deps) : new List<string>();
        }

        /// <summary>
        /// 特定システムに依存するシステムを取得
        /// </summary>
        public List<string> GetDependentSystems(string systemName)
        {
            var dependents = new List<string>();
            
            foreach (var (system, deps) in dependencies)
            {
                if (deps.Contains(systemName))
                {
                    dependents.Add(system);
                }
            }

            return dependents;
        }
    }
}