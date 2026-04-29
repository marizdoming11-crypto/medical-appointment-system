<?php

namespace App\Http\Controllers;

use Illuminate\Http\Request;
use Illuminate\Support\Facades\Http;

class AuthController extends Controller
{
    private string $api;

    public function __construct()
    {
        $this->api = env('API_BASE_URL', 'http://localhost:8001/api');
    }

    public function login()
    {
        return view('auth.login');
    }

    public function loginPost(Request $request)
    {
        $request->validate(['username' => 'required|min:3', 'password' => 'required|min:6']);
        $response = Http::post($this->api . '/Auth/login', $request->only('username', 'password'));
        if (!$response->successful()) return back()->with('error', $response->json('detail') ?? 'Invalid login.');
        $user = $response->json();
        session(['token' => $user['access_token'], 'user_id' => $user['id'], 'full_name' => $user['fullName'], 'role' => $user['role']]);
        return $user['role'] === 'Admin' ? redirect('/admin/dashboard') : redirect('/user/dashboard');
    }

    public function register()
    {
        return view('auth.register');
    }

    public function registerPost(Request $request)
    {
        $request->validate(['fullName' => 'required|min:2', 'username' => 'required|min:3', 'password' => 'required|min:6']);
        $response = Http::post($this->api . '/Auth/register', $request->only('fullName', 'username', 'password'));
        if (!$response->successful()) return back()->with('error', $response->json('detail') ?? $response->body());
        return redirect('/login')->with('success', 'Registration successful.');
    }

    public function logout()
    {
        session()->flush();
        return redirect('/login')->with('success', 'Signed out.');
    }
}
