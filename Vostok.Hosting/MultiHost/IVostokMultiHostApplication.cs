using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Models;

namespace Vostok.Hosting.MultiHost
{
    /// <summary>
    /// <para>An <see cref="IVostokApplication"/> launcher.</para>
    /// <para>Used in <see cref="VostokMultiHost"/> and responsible for doing the following:</para>
    /// <list type="bullet">
    ///     <item><description>Creating an instance of <see cref="IVostokHostingEnvironment"/> using <see cref="VostokMultiHostApplicationSettings.EnvironmentSetup"/>.</description></item>
    ///     <item><description>Running the application by calling <see cref="IVostokApplication.InitializeAsync"/> and then <see cref="IVostokApplication.RunAsync"/>.</description></item>
    /// </list>
    /// </summary>
    [PublicAPI]
    public interface IVostokMultiHostApplication
    {
        /// <summary>
        /// An application unique identifier. Used as default for <see cref="IVostokApplicationIdentity.Application"/> and <see cref="IVostokApplicationIdentity.Instance"/>.;
        /// </summary>
        VostokMultiHostApplicationIdentifier Identifier { get; }

        /// <summary>
        /// <inheritdoc cref="VostokHost.ApplicationState"/>
        /// </summary>
        VostokApplicationState ApplicationState { get; }

        /// <summary>
        /// <para>Launches the provided <see cref="IVostokApplication"/>.</para>
        /// <para>If called for the first time, performs following operations:</para>
        /// <list type="bullet">
        ///     <item><description>Creates an instance of <see cref="IVostokHostingEnvironment"/> using <see cref="VostokMultiHostApplicationSettings.EnvironmentSetup"/>.</description></item>
        ///     <item><description>Calls <see cref="IVostokApplication.InitializeAsync"/>.</description></item>
        ///     <item><description>Calls <see cref="IVostokApplication.RunAsync"/>.</description></item>
        /// </list>
        /// <para>Returns the same cached task on next calls.</para>
        /// <para>Throws exception if parent <see cref="VostokMultiHost"/> was not started.</para>
        /// <para>Does not rethrow exceptions from <see cref="IVostokApplication"/>, stores them in result's <see cref="VostokApplicationRunResult.Error"/> property.</para>
        /// </summary>
        Task<VostokApplicationRunResult> RunAsync();

        /// <summary>
        /// <para>If called for the first time, starts the execution of the application and optionally waits for given state to occur.</para>
        /// <para>Waits for given state on next calls.</para>
        /// <para>Throws exception if parent <see cref="VostokMultiHost"/> was not started.</para>
        /// <para>If not given a <paramref name="stateToAwait"/>, acts in a fire-and-forget fashion.</para>
        /// <para>If given <paramref name="stateToAwait"/> is not reached before the task returned by
        /// <see cref="RunAsync"/> completes, simply awaits that task instead, propagating its error in case of crash.</para>
        /// <para>Waits for the <see cref="VostokApplicationState.Running"/> state by default.</para>
        /// </summary>
        Task StartAsync(VostokApplicationState? stateToAwait = VostokApplicationState.Running);

        /// <summary>
        /// <para>Cancels the execution of the application.</para>
        /// <para>If <paramref name="ensureSuccess"/> is <c>true</c> (which is the default), propagates errors from app crashes.</para>
        /// </summary>
        Task<VostokApplicationRunResult> StopAsync(bool ensureSuccess = true);
    }
}