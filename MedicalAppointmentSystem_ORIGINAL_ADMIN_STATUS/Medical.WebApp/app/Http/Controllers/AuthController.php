<?php
namespace App\Http\Controllers;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Http;

class AuthController extends Controller {
    private string $api;
    public function __construct(){ $this->api = env('API_BASE_URL', 'http://127.0.0.1:8001/api'); }
    public function login(){ return view('auth.login'); }
    public function loginPost(Request $r){
        $r->validate(['username'=>'required|min:3','password'=>'required|min:6']);
        $res = Http::post($this->api.'/Auth/login', ['username'=>$r->username, 'password'=>$r->password]);
        if(!$res->successful()) return back()->with('error', $res->json('detail') ?? 'Invalid username or password.');
        $u = $res->json();
        session(['token'=>$u['access_token'], 'user_id'=>$u['id'], 'full_name'=>$u['fullName'], 'role'=>$u['role']]);
        return $u['role']==='Admin' ? redirect('/admin/dashboard') : redirect('/user/dashboard');
    }
    public function register(){ return view('auth.register'); }
    public function registerPost(Request $r){
        $r->validate(['fullName'=>'required|min:2','username'=>'required|min:3','password'=>'required|min:6']);
        $res = Http::post($this->api.'/Auth/register', ['fullName'=>$r->fullName, 'username'=>$r->username, 'password'=>$r->password]);
        if(!$res->successful()) return back()->with('error', $res->json('detail') ?? $res->body());
        return redirect('/login')->with('success','Registration successful.');
    }
    public function logout(){ session()->flush(); return redirect('/login')->with('success','Signed out.'); }
}
