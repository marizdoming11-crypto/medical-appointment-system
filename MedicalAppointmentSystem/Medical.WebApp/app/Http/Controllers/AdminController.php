<?php

namespace App\Http\Controllers;

use Illuminate\Http\Request;
use Illuminate\Support\Facades\Http;

class AdminController extends Controller
{
    private string $api;
    public function __construct() { $this->api = env('API_BASE_URL', 'http://localhost:8001/api'); }
    private function guard() { return session('role') === 'Admin' && session('token') ? null : redirect('/login')->with('error', 'Admin access required.'); }
    private function auth() { return Http::withToken(session('token')); }

    public function dashboard() { if ($r = $this->guard()) return $r; return view('admin.dashboard'); }
    public function doctors(Request $request) { if ($r = $this->guard()) return $r; $doctors = Http::get($this->api . '/Doctors', ['search' => $request->search])->json(); return view('admin.doctors', compact('doctors')); }
    public function addDoctor() { if ($r = $this->guard()) return $r; return view('admin.add-doctor'); }
    public function storeDoctor(Request $request) { if ($r = $this->guard()) return $r; $request->validate(['fullName'=>'required|min:2','specialization'=>'required|min:2','contactNumber'=>'required|min:5']); $res=$this->auth()->post($this->api.'/Doctors',$request->only('fullName','specialization','contactNumber')); return $res->successful()?redirect('/admin/doctors')->with('success','Doctor added.'):back()->with('error',$res->json('detail')??$res->body()); }
    public function editDoctor($id) { if ($r = $this->guard()) return $r; $doctor = Http::get($this->api . '/Doctors/' . $id)->json(); return view('admin.edit-doctor', compact('doctor')); }
    public function updateDoctor(Request $request,$id) { if ($r = $this->guard()) return $r; $request->validate(['fullName'=>'required|min:2','specialization'=>'required|min:2','contactNumber'=>'required|min:5']); $res=$this->auth()->put($this->api.'/Doctors/'.$id,$request->only('fullName','specialization','contactNumber')); return $res->successful()?redirect('/admin/doctors')->with('success','Doctor updated.'):back()->with('error',$res->json('detail')??$res->body()); }
    public function deleteDoctor($id) { if ($r = $this->guard()) return $r; $res=$this->auth()->delete($this->api.'/Doctors/'.$id); return redirect('/admin/doctors')->with($res->successful()?'success':'error',$res->successful()?'Doctor deleted.':($res->json('detail')??$res->body())); }
    public function schedules() { if ($r = $this->guard()) return $r; $schedules = Http::get($this->api . '/DoctorSchedules')->json(); return view('admin.schedules', compact('schedules')); }
    public function addSchedule() { if ($r = $this->guard()) return $r; $doctors = Http::get($this->api . '/Doctors')->json(); return view('admin.add-schedule', compact('doctors')); }
    public function storeSchedule(Request $request) { if ($r = $this->guard()) return $r; $request->validate(['doctorId'=>'required','scheduleDate'=>'required|date','startTime'=>'required','endTime'=>'required','startTime24'=>'required','endTime24'=>'required']); $data=$request->only('doctorId','scheduleDate','startTime','endTime','startTime24','endTime24'); $data['doctorId']=(int)$data['doctorId']; $data['isAvailable']=true; $res=$this->auth()->post($this->api.'/DoctorSchedules',$data); return $res->successful()?redirect('/admin/schedules')->with('success','Schedule added.'):back()->with('error',$res->json('detail')??$res->body()); }
    public function deleteSchedule($id) { if ($r = $this->guard()) return $r; $res=$this->auth()->delete($this->api.'/DoctorSchedules/'.$id); return redirect('/admin/schedules')->with($res->successful()?'success':'error',$res->successful()?'Schedule deleted.':($res->json('detail')??$res->body())); }
    public function appointments() { if ($r = $this->guard()) return $r; $appointments = $this->auth()->get($this->api . '/Appointments')->json(); return view('admin.appointments', compact('appointments')); }
}
