<?php
namespace App\Http\Middleware;
use Closure;
use Illuminate\Http\Request;
class AdminMiddleware {
    public function handle(Request $request, Closure $next) {
        if (session('role') !== 'Admin' || !session('token')) return redirect('/login')->with('error', 'Admin access required.');
        return $next($request);
    }
}
