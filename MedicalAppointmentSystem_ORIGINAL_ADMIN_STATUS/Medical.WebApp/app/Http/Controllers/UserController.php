<?php
namespace App\Http\Controllers;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Http;

class UserController extends Controller {
    private string $api;
    public function __construct(){ $this->api = env('API_BASE_URL', 'http://127.0.0.1:8001/api'); }
    private function http(){ return Http::withToken(session('token')); }
    public function dashboard(){ return view('user.dashboard'); }
    public function doctors(){ $doctors = $this->http()->get($this->api.'/Doctors')->json() ?? []; return view('user.doctors', compact('doctors')); }
    public function appointments(){ $appointments=$this->http()->get($this->api.'/Appointments/user/'.session('user_id'))->json()??[]; return view('user.appointments', compact('appointments')); }
    public function createAppointment(){ $doctors=$this->http()->get($this->api.'/Doctors')->json()??[]; return view('user.create-appointment', compact('doctors')); }
    public function storeAppointment(Request $r){
        $r->validate(['doctorId'=>'required','appointmentDate'=>'required','reason'=>'required|min:2']);
        $res = $this->http()->post($this->api.'/Appointments', ['userId'=>(int)session('user_id'),'doctorId'=>(int)$r->doctorId,'appointmentDate'=>$r->appointmentDate,'reason'=>$r->reason,'status'=>'Pending']);
        return $res->successful() ? redirect('/user/appointments')->with('success','Appointment booked.') : back()->with('error',$res->json('detail')??$res->body());
    }
    public function editAppointment($id){ $appointments=$this->http()->get($this->api.'/Appointments/user/'.session('user_id'))->json()??[]; $appointment=collect($appointments)->firstWhere('id',(int)$id); $doctors=$this->http()->get($this->api.'/Doctors')->json()??[]; return view('user.edit-appointment', compact('appointment','doctors')); }
    public function updateAppointment(Request $r,$id){
        $r->validate(['doctorId'=>'required','appointmentDate'=>'required','reason'=>'required|min:2']);
        $res = $this->http()->put($this->api.'/Appointments/update/'.$id, ['doctorId'=>(int)$r->doctorId,'appointmentDate'=>$r->appointmentDate,'reason'=>$r->reason]);
        return $res->successful() ? redirect('/user/appointments')->with('success','Appointment updated.') : back()->with('error',$res->json('detail')??$res->body());
    }
    public function cancelAppointment($id){ $res=$this->http()->put($this->api.'/Appointments/cancel/'.$id); return redirect('/user/appointments')->with($res->successful()?'success':'error', $res->successful()?'Appointment cancelled.':($res->json('detail')??$res->body())); }
}
