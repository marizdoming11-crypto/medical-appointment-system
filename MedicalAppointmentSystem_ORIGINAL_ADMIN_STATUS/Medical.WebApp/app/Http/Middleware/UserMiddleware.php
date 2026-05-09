<?php
namespace App\Http\Middleware;
use Closure;
use Illuminate\Http\Request;
class UserMiddleware {
    public function handle(Request $request, Closure $next) {
        if (session('role') !== 'User' || !session('token')) return redirect('/login')->with('error', 'User access required.');
        return $next($request);
    }
}
