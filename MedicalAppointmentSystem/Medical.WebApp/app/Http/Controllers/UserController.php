<?php

namespace App\Http\Controllers;

use Illuminate\Http\Request;
use Illuminate\Support\Facades\Http;

class UserController extends Controller
{
    private string $api;
    public function __construct() { $this->api = env('API_BASE_URL', 'http://localhost:8001/api'); }
    private function guard() { return session('role') === 'User' && session('token') ? null : redirect('/login')->with('error', 'User access required.'); }
    private function auth() { return Http::withToken(session('token')); }

    public function dashboard() { if ($r = $this->guard()) return $r; return view('user.dashboard'); }
    public function doctors() { if ($r = $this->guard()) return $r; $doctors = Http::get($this->api . '/Doctors')->json(); return view('user.doctors', compact('doctors')); }
    public function appointments() { if ($r = $this->guard()) return $r; $appointments = $this->auth()->get($this->api . '/Appointments/user/' . session('user_id'))->json(); return view('user.appointments', compact('appointments')); }
    public function createAppointment() { if ($r = $this->guard()) return $r; $doctors = Http::get($this->api . '/Doctors')->json(); return view('user.create-appointment', compact('doctors')); }
    public function storeAppointment(Request $request) { if ($r = $this->guard()) return $r; $request->validate(['doctorId'=>'required','appointmentDate'=>'required|date','appointmentTime'=>'required','reason'=>'required|min:2']); $data=['userId'=>(int)session('user_id'),'doctorId'=>(int)$request->doctorId,'appointmentDate'=>$request->appointmentDate,'appointmentTime'=>$request->appointmentTime,'reason'=>$request->reason,'status'=>'Pending']; $res=$this->auth()->post($this->api.'/Appointments',$data); return $res->successful()?redirect('/user/appointments')->with('success','Appointment created.'):back()->with('error',$res->json('detail')??$res->body()); }
    public function cancelAppointment($id) { if ($r = $this->guard()) return $r; $res=$this->auth()->put($this->api.'/Appointments/cancel/'.$id); return redirect('/user/appointments')->with($res->successful()?'success':'error',$res->successful()?'Appointment cancelled.':($res->json('detail')??$res->body())); }
}
